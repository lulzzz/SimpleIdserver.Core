using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Website.PendingRequestsController.Actions;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.PendingRequestsController
{
    public interface IPendingRequestsActions
    {
        Task<SearchPendingRequestResult> GetPendingRequests(GetPendingRequestsParameter parameter);
        Task<bool> Accept(string subject, string id);
        Task<bool> Reject(string subject, string id);
    }

    internal sealed class PendingRequestsActions : IPendingRequestsActions
    {
        private readonly IGetPendingRequestsAction _getPendingRequestsAction;
        private readonly IAcceptPendingRequestAction _acceptPendingRequestAction;
        private readonly IRejectPendingRequestAction _rejectPendingRequestAction;

        public PendingRequestsActions(IGetPendingRequestsAction getPendingRequestsAction, IAcceptPendingRequestAction acceptPendingRequestAction, IRejectPendingRequestAction rejectPendingRequestAction)
        {
            _getPendingRequestsAction = getPendingRequestsAction;
            _acceptPendingRequestAction = acceptPendingRequestAction;
            _rejectPendingRequestAction = rejectPendingRequestAction;
        }

        public Task<SearchPendingRequestResult> GetPendingRequests(GetPendingRequestsParameter parameter)
        {
            return _getPendingRequestsAction.Execute(parameter);
        }

        public Task<bool> Accept(string subject, string id)
        {
            return _acceptPendingRequestAction.Execute(subject, id);
        }

        public Task<bool> Reject(string subject, string id)
        {
            return _rejectPendingRequestAction.Execute(subject, id);
        }
    }
}
