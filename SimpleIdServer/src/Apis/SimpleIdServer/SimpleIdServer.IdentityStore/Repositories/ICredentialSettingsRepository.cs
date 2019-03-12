using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    public interface ICredentialSettingsRepository
    {
        Task<IEnumerable<CredentialSetting>> Get();
        Task<CredentialSetting> Get(string type);
        Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types);
        Task<bool> Update(CredentialSetting credentialSetting);
    }
}