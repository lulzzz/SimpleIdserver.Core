using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    public interface IUserCredentialRepository
    {
        Task<bool> Add(IEnumerable<UserCredential> userCredentials);
        Task<UserCredential> Get(string type, string value);
        Task<UserCredential> GetUserCredential(string subject, string type);
        Task<bool> Update(UserCredential resourceOwnerCredential);
        Task<bool> Delete(string subject, string type);
    }
}
