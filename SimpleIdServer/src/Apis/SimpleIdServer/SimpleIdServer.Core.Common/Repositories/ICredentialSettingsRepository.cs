using SimpleIdServer.Core.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface ICredentialSettingsRepository
    {
        Task<IEnumerable<CredentialSetting>> Get();
        Task<CredentialSetting> Get(string type);
        Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types);
        Task<bool> Update(CredentialSetting credentialSetting);
    }
}