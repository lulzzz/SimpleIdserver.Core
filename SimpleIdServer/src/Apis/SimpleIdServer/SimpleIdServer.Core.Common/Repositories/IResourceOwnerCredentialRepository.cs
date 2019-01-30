
using SimpleIdServer.Core.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface IResourceOwnerCredentialRepository
    {
        Task<bool> Add(IEnumerable<ResourceOwnerCredential> resourceOwnerCredentials);
        Task<ResourceOwnerCredential> Get(string type, string value);
        Task<ResourceOwnerCredential> GetUserCredential(string subject, string type);
        Task<bool> Update(ResourceOwnerCredential resourceOwnerCredential);
    }
}
