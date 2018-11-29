using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public interface IResourceSetRepository
    {
        Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter);
        Task<bool> Insert(ResourceSet resourceSet);
        Task<ResourceSet> Get(string id);
        Task<bool> Update(ResourceSet resourceSet);
        Task<ICollection<ResourceSet>> GetAll();
        Task<bool> Delete(string id);
        Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids);
    }
}