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
    }
}
