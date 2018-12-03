#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;

namespace SimpleIdServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IUpdatePolicyAction
    {
        Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter);
    }

    internal class UpdatePolicyAction : IUpdatePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public UpdatePolicyAction(IPolicyRepository policyRepository, IRepositoryExceptionHelper repositoryExceptionHelper,
            IResourceSetRepository resourceSetRepository, IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }
        
        public async Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter)
        {
            if (updatePolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(updatePolicyParameter));
            }

            if (string.IsNullOrWhiteSpace(updatePolicyParameter.PolicyId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }
            
            _umaServerEventSource.StartUpdateAuthorizationPolicy(JsonConvert.SerializeObject(updatePolicyParameter));
            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                () => _policyRepository.Get(updatePolicyParameter.PolicyId));
            if (policy == null)
            {
                return false;
            }
            
            foreach (var resourceSetId in policy.ResourceSetIds)
            {
                var resourceSet = await _resourceSetRepository.Get(resourceSetId).ConfigureAwait(false);
                if (updatePolicyParameter.Scopes.Any(r => !resourceSet.Scopes.Contains(r)))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidScope, ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            policy.Scopes = updatePolicyParameter.Scopes;
            policy.IsResourceOwnerConsentNeeded = updatePolicyParameter.IsResourceOwnerConsentNeeded;
            policy.Claims = new List<Claim>();
            policy.Script = updatePolicyParameter.Script;
            if (updatePolicyParameter.Claims != null)
            {
                policy.Claims = updatePolicyParameter.Claims.Select(c => new Claim
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList();
            }

            var result = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, updatePolicyParameter.PolicyId),
                () => _policyRepository.Update(policy));
            _umaServerEventSource.FinishUpdateAuhthorizationPolicy(JsonConvert.SerializeObject(updatePolicyParameter));
            return result;
        }
    }
}
