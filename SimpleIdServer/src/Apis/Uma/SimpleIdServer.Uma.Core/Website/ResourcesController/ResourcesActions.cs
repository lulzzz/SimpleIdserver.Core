using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Website.ResourcesController.Actions;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController
{
    public interface IResourcesActions
    {
        Task<SearchResourceSetResult> GetCurrentUserResources(SearchCurrentUserResourceSetParameter parameter);
    }

    internal sealed class ResourcesActions : IResourcesActions
    {
        private readonly IGetCurrentUserResourcesAction _getCurrentUserResourcesAction;

        public ResourcesActions(IGetCurrentUserResourcesAction getCurrentUserResourcesAction)
        {
            _getCurrentUserResourcesAction = getCurrentUserResourcesAction;
        }

        public Task<SearchResourceSetResult> GetCurrentUserResources(SearchCurrentUserResourceSetParameter parameter)
        {
            return _getCurrentUserResourcesAction.Execute(parameter);
        }
    }
}