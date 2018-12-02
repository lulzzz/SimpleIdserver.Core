using SimpleIdServer.Uma.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public interface IPendingRequestRepository
    {
        Task<bool> Add(PendingRequest pendingRequest);
        Task<PendingRequest> Get(string policyRuleId, string subject);
    }
}
