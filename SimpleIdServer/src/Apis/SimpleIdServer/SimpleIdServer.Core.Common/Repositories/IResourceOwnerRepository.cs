using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.Core.Common.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface IResourceOwnerRepository
    {
        Task<ResourceOwner> GetResourceOwnerByClaim(string key, string value);
        Task<ResourceOwner> GetAsync(string id);
        Task<ICollection<ResourceOwner>> GetAsync(IEnumerable<System.Security.Claims.Claim> claims);
        Task<ICollection<ResourceOwner>> GetAllAsync();
        Task<bool> InsertAsync(ResourceOwner resourceOwner);
        Task<bool> UpdateAsync(ResourceOwner resourceOwner);
        Task<bool> UpdateCredential(string subject, ResourceOwnerCredential credential);
        Task<bool> DeleteAsync(string subject);
        Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter);
    }
}