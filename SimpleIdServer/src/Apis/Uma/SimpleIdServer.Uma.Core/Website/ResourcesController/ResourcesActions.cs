using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Website.ResourcesController.Actions;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController
{
    public interface IResourcesActions
    {
        Task<SearchResourceSetResult> GetCurrentUserResources(SearchCurrentUserResourceSetParameter parameter);
        Task<bool> ConfirmSharedLink(ConfirmSharedLinkParameter confirmSharedLinkParameter);
        Task<string> ShareResource(ShareResourceParameter shareResourceParameter);
        Task<bool> UpdateResourcePermissions(UpdateResourcePermissionsParameter updateResourcePermissionsParameter);
        Task<SearchResourceSetResult> GetResourcesSharedWith(SearchSharedResourcesParameter subject);
    }

    internal sealed class ResourcesActions : IResourcesActions
    {
        private readonly IGetCurrentUserResourcesAction _getCurrentUserResourcesAction;
        private readonly IConfirmSharedLinkAction _confirmSharedLinkAction;
        private readonly IShareResourceAction _shareResourceAction;
        private readonly IUpdateResourcePermissionsAction _updateResourcePermissionsAction;
        private readonly IGetResourcesSharedWithMeAction _getResourcesSharedWithMeAction;

        public ResourcesActions(IGetCurrentUserResourcesAction getCurrentUserResourcesAction, IConfirmSharedLinkAction confirmSharedLinkAction, IShareResourceAction shareResourceAction, IUpdateResourcePermissionsAction updateResourcePermissionsAction,
            IGetResourcesSharedWithMeAction getResourcesSharedWithMeAction)
        {
            _getCurrentUserResourcesAction = getCurrentUserResourcesAction;
            _confirmSharedLinkAction = confirmSharedLinkAction;
            _shareResourceAction = shareResourceAction;
            _updateResourcePermissionsAction = updateResourcePermissionsAction;
            _getResourcesSharedWithMeAction = getResourcesSharedWithMeAction;
        }

        public Task<SearchResourceSetResult> GetCurrentUserResources(SearchCurrentUserResourceSetParameter parameter)
        {
            return _getCurrentUserResourcesAction.Execute(parameter);
        }

        public Task<bool> ConfirmSharedLink(ConfirmSharedLinkParameter confirmSharedLinkParameter)
        {
            return _confirmSharedLinkAction.Execute(confirmSharedLinkParameter);
        }

        public Task<string> ShareResource(ShareResourceParameter shareResourceParameter)
        {
            return _shareResourceAction.Execute(shareResourceParameter);
        }

        public Task<bool> UpdateResourcePermissions(UpdateResourcePermissionsParameter updateResourcePermissionsParameter)
        {
            return _updateResourcePermissionsAction.Execute(updateResourcePermissionsParameter);
        }

        public Task<SearchResourceSetResult> GetResourcesSharedWith(SearchSharedResourcesParameter parameter)
        {
            return _getResourcesSharedWithMeAction.Execute(parameter);
        }
    }
}