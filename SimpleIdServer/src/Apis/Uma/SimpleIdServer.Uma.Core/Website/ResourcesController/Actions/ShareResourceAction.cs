using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController.Actions
{
    public interface IShareResourceAction
    {
        Task<string> Execute(ShareResourceParameter shareResourceParameter);
    }

    internal sealed class ShareResourceAction : IShareResourceAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ISharedLinkRepository _sharedLinkRepository;

        public ShareResourceAction(IResourceSetRepository resourceSetRepository, ISharedLinkRepository sharedLinkRepository)
        {
            _resourceSetRepository = resourceSetRepository;
            _sharedLinkRepository = sharedLinkRepository;
        }

        public async Task<string> Execute(ShareResourceParameter shareResourceParameter)
        {
            if (shareResourceParameter == null)
            {
                throw new ArgumentNullException(nameof(shareResourceParameter));
            }

            if (string.IsNullOrWhiteSpace(shareResourceParameter.ResourceId))
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheResourceIdMustBeSpecified);
            }

            if (shareResourceParameter.Scopes == null)
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheScopesMustBeSpecified);
            }

            var resource = await _resourceSetRepository.Get(shareResourceParameter.ResourceId).ConfigureAwait(false);
            if (resource == null)
            {
                throw new UmaResourceNotFoundException();
            }

            if (!shareResourceParameter.Scopes.All(s => resource.Scopes.Contains(s)))
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheScopeAreNotValid);
            }

            if (shareResourceParameter.Owner != shareResourceParameter.Owner)
            {
                throw new UmaNotAuthorizedException();
            }

            var sharedLink = new SharedLink
            {
                ConfirmationCode = Guid.NewGuid().ToString(),
                ResourceId = resource.Id,
                Scopes = shareResourceParameter.Scopes
            };
            if (!await _sharedLinkRepository.Insert(sharedLink))
            {
                throw new BaseUmaException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheSharedLinkCannotBeInserted);
            }

            return sharedLink.ConfirmationCode;
        }
    }
}
