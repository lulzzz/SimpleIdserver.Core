using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class UserCredentialRepository : IUserCredentialRepository
    {
        public Task<bool> Add(IEnumerable<UserCredential> userCredentials)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Delete(string subject, string type)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserCredential> Get(string type, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserCredential> GetUserCredential(string subject, string type)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(UserCredential resourceOwnerCredential)
        {
            throw new System.NotImplementedException();
        }
    }
}
