using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public interface IPendingRequestRepository
    {
        Task<bool> Add(PendingRequest pendingRequest);
        Task<PendingRequest> Get(string id);
        Task<IEnumerable<PendingRequest>> Get(string resourceId, string subject);
        Task<bool> Update(PendingRequest pendingRequest);
        Task<SearchPendingRequestResult> Search(SearchPendingRequestParameter parameter);
        Task<bool> Delete(string id);
    }
}