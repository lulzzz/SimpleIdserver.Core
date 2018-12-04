using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.PendingRequestsController.Actions
{
    public interface IRejectPendingRequestAction
    {
        Task<bool> Execute(string subject, string id);
    }

    internal sealed class RejectPendingRequestAction : IRejectPendingRequestAction
    {
        private readonly IPendingRequestRepository _pendingRequestRepository;

        public RejectPendingRequestAction(IPendingRequestRepository pendingRequestRepository)
        {
            _pendingRequestRepository = pendingRequestRepository;
        }
        
        public async Task<bool> Execute(string subject, string id)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _pendingRequestRepository.Get(id).ConfigureAwait(false);
            if (result == null)
            {
                throw new UmaPolicyNotFoundException();
            }

            if (result.Resource.Owner != subject)
            {
                throw new UmaNotAuthorizedException();
            }

            await _pendingRequestRepository.Delete(id).ConfigureAwait(false);
            return true;
        }
    }
}
