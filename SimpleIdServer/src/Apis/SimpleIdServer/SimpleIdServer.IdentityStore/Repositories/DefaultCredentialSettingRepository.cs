using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    internal sealed class DefaultCredentialSettingRepository : ICredentialSettingsRepository
    {
        private readonly IEnumerable<CredentialSetting> _credentialSettings;

        public DefaultCredentialSettingRepository(IEnumerable<CredentialSetting> credentialSettings)
        {
            _credentialSettings = credentialSettings == null ? new List<CredentialSetting>() : credentialSettings;
        }

        public Task<IEnumerable<CredentialSetting>> Get()
        {
            return Task.FromResult(_credentialSettings);
        }

        public Task<CredentialSetting> Get(string type)
        {
            return Task.FromResult(_credentialSettings.FirstOrDefault(r => r.CredentialType == type));
        }

        public Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types)
        {
            return Task.FromResult(_credentialSettings.Where(c => types.Contains(c.CredentialType)));
        }

        public Task<bool> Update(CredentialSetting credentialSetting)
        {
            var record = _credentialSettings.FirstOrDefault(r => r.CredentialType == credentialSetting.CredentialType);
            if (record == null)
            {
                return Task.FromResult(false);
            }

            record.AuthenticationIntervalsInSeconds = credentialSetting.AuthenticationIntervalsInSeconds;
            record.IsBlockAccountPolicyEnabled = credentialSetting.IsBlockAccountPolicyEnabled;
            record.NumberOfAuthenticationAttempts = credentialSetting.NumberOfAuthenticationAttempts;
            record.ExpiresIn = credentialSetting.ExpiresIn;
            record.Options = credentialSetting.Options;
            return Task.FromResult(true);
        }
    }
}
