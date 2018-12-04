using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.PendingRequestsController.Actions
{
    public interface IGetPendingRequestsAction
    {
        Task<SearchPendingRequestResult> Execute(GetPendingRequestsParameter parameter);
    }

    internal sealed class GetPendingRequestsAction : IGetPendingRequestsAction
    {
        private readonly IPendingRequestRepository _pendingRequestRepository;

        public GetPendingRequestsAction(IPendingRequestRepository pendingRequestRepository)
        {
            _pendingRequestRepository = pendingRequestRepository;
        }

        public Task<SearchPendingRequestResult> Execute(GetPendingRequestsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _pendingRequestRepository.Search(new SearchPendingRequestParameter
            {
                IsPagingEnabled = true,
                Count = parameter.Count,
                StartIndex = parameter.StartIndex,
                Owners = new[] { parameter.Subject },
                IsConfirmed = false
            });
        }
    }
}
