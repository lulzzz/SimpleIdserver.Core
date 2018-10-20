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
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;

namespace SimpleIdServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IDeleteAuthorizationPolicyAction
    {
        Task<bool> Execute(string policyId);
    }

    internal class DeleteAuthorizationPolicyAction : IDeleteAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public DeleteAuthorizationPolicyAction(
            IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(string policyId)
        {
            _umaServerEventSource.StartToRemoveAuthorizationPolicy(policyId);
            if (string.IsNullOrWhiteSpace(policyId))
            {
                throw new ArgumentNullException(nameof(policyId));
            }

            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                () => _policyRepository.Get(policyId));
            if (policy == null)
            {
                return false;
            }

            await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, policyId),
                () => _policyRepository.Delete(policyId));
            _umaServerEventSource.FinishToRemoveAuthorizationPolicy(policyId);
            return true;
        }
    }
}
