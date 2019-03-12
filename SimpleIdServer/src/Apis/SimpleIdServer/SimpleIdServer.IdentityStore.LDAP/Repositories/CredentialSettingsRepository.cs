using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class CredentialSettingsRepository : ICredentialSettingsRepository
    {
        public Task<IEnumerable<CredentialSetting>> Get()
        {
            throw new System.NotImplementedException();
        }

        public Task<CredentialSetting> Get(string type)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(CredentialSetting credentialSetting)
        {
            throw new System.NotImplementedException();
        }
    }
}
