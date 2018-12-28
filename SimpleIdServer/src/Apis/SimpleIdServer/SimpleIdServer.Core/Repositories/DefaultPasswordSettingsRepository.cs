using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Repositories
{
    internal sealed class DefaultPasswordSettingsRepository : IPasswordSettingsRepository
    {
        private readonly PasswordSettings _passwordSettings;

        public DefaultPasswordSettingsRepository(PasswordSettings passwordSettings)
        {
            _passwordSettings = passwordSettings == null ? new PasswordSettings() : passwordSettings;
        }

        public Task<PasswordSettings> Get()
        {
            return Task.FromResult(_passwordSettings);
        }

        public Task<bool> Update(PasswordSettings passwordSettings)
        {
            _passwordSettings.AuthenticationIntervalsInSeconds = passwordSettings.AuthenticationIntervalsInSeconds;
            _passwordSettings.IsBlockAccountPolicyEnabled = passwordSettings.IsBlockAccountPolicyEnabled;
            _passwordSettings.IsRegexEnabled = passwordSettings.IsRegexEnabled;
            _passwordSettings.NumberOfAuthenticationAttempts = passwordSettings.NumberOfAuthenticationAttempts;
            _passwordSettings.PasswordDescription = passwordSettings.PasswordDescription;
            _passwordSettings.PasswordExpiresIn = passwordSettings.PasswordExpiresIn;
            _passwordSettings.RegularExpression = passwordSettings.RegularExpression;
            return Task.FromResult(true);
        }
    }
}
