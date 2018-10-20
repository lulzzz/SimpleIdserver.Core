using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Parameters;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface IProfileRepository
    {
        Task<ResourceOwnerProfile> Get(string subject);
        Task<bool> Add(IEnumerable<ResourceOwnerProfile> profiles);
        Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter);
        Task<bool> Remove(IEnumerable<string> subjects);
    }
}