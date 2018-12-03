using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;

namespace SimpleIdServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IDeleteResourcePolicyAction
    {
        Task<bool> Execute(string id, string resourceId);
    }

    internal class DeleteResourcePolicyAction : IDeleteResourcePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public DeleteResourcePolicyAction(
            IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }
        
        public async Task<bool> Execute(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            _umaServerEventSource.StartRemoveResourceFromAuthorizationPolicy(id, resourceId);
            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, id),
                () => _policyRepository.Get(id));
            if (policy == null)
            {
                return false;
            }

            var resourceSet = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId),
                () => _resourceSetRepository.Get(resourceId));
            if (resourceSet == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidResourceSetId,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
            }

            if (policy.ResourceSetIds == null || !policy.ResourceSetIds.Contains(resourceId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidResourceSetId,
                    ErrorDescriptions.ThePolicyDoesntContainResource);
            }

            var resourceSetIds = policy.ResourceSetIds.ToList();
            resourceSetIds.Remove(resourceId);
            policy.ResourceSetIds = resourceSetIds;
            var result = await _policyRepository.Update(policy).ConfigureAwait(false);
            _umaServerEventSource.FinishRemoveResourceFromAuthorizationPolicy(id, resourceId);
            return result;
        }
    }
}
