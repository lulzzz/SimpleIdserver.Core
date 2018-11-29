using SimpleIdServer.Uma.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public interface ISharedLinkRepository
    {
        Task<bool> Insert(SharedLink sharedLink);
        Task<SharedLink> Get(string confirmationCode);
        Task<bool> Delete(string confirmationCode);
    }
}