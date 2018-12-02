using SimpleIdServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdServer.Uma.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.PermissionController
{
    public interface IPermissionControllerActions
    {
        Task<string> Add(IEnumerable<string> audiences, AddPermissionParameter addPermissionParameter);
        Task<string> Add(IEnumerable<string> audiences, IEnumerable<AddPermissionParameter> addPermissionParameters);
    }

    internal class PermissionControllerActions : IPermissionControllerActions
    {
        private readonly IAddPermissionAction _addPermissionAction;

        public PermissionControllerActions(IAddPermissionAction addPermissionAction)
        {
            _addPermissionAction = addPermissionAction;
        }

        public Task<string> Add(IEnumerable<string> audiences, AddPermissionParameter addPermissionParameter)
        {
            return _addPermissionAction.Execute(audiences, addPermissionParameter);
        }

        public Task<string> Add(IEnumerable<string> audiences, IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            return _addPermissionAction.Execute(audiences, addPermissionParameters);
        }
    }
}
