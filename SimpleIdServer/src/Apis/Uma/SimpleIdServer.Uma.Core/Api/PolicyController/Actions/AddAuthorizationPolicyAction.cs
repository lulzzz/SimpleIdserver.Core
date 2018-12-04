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

using Newtonsoft.Json;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IAddAuthorizationPolicyAction
    {
        Task<string> Execute(AddPolicyParameter addPolicyParameter);
    }

    internal class AddAuthorizationPolicyAction : IAddAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AddAuthorizationPolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerEventSource = umaServerEventSource;
        }
        
        public async Task<string> Execute(AddPolicyParameter addPolicyParameter)
        {
            var json = addPolicyParameter == null ? string.Empty : JsonConvert.SerializeObject(addPolicyParameter);
            _umaServerEventSource.StartAddingAuthorizationPolicy(json);
            if (addPolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(addPolicyParameter));
            }

            if (addPolicyParameter.ResourceSetIds == null || !addPolicyParameter.ResourceSetIds.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.ResourceSetIds));
            }
            
            foreach (var resourceSetId in addPolicyParameter.ResourceSetIds)
            {
                var resourceSet = await _repositoryExceptionHelper.HandleException(
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId),
                    () => _resourceSetRepository.Get(resourceSetId));
                if (resourceSet == null)
                {
                    throw new BaseUmaException(ErrorCodes.InvalidResourceSetId, string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }

                if (addPolicyParameter.Scopes.Any(r => !resourceSet.Scopes.Contains(r)))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidScope, ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            // Insert policy
            var policy = new Policy
            {
                Id = Guid.NewGuid().ToString(),
                ClientIds = addPolicyParameter.ClientIdsAllowed,
                ResourceSetIds = addPolicyParameter.ResourceSetIds,
                IsResourceOwnerConsentNeeded = addPolicyParameter.IsResourceOwnerConsentNeeded,
                Script = addPolicyParameter.Script,
                Scopes = addPolicyParameter.Scopes,
                Claims = addPolicyParameter.Claims == null ? new List<Claim>() : addPolicyParameter.Claims.Select(c => new Claim
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            await _repositoryExceptionHelper.HandleException(ErrorDescriptions.ThePolicyCannotBeInserted, () => _policyRepository.Add(policy));
            _umaServerEventSource.FinishToAddAuthorizationPolicy(JsonConvert.SerializeObject(policy));
            return policy.Id;
        }
    }
}
