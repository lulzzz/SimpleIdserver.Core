using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.EF.Repositories
{
    internal sealed class CredentialsSettingsRepository : ICredentialSettingsRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public CredentialsSettingsRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CredentialSetting>> Get()
        {
            var result = await _context.CredentialSettings.ToListAsync().ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.Select(s => s.ToDomain());
        }

        public async Task<CredentialSetting> Get(string type)
        {
            var result = await _context.CredentialSettings.FirstOrDefaultAsync(p => p.CredentialType == type).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types)
        {
            var result = await _context.CredentialSettings.Where(p => types.Contains(p.CredentialType)).ToListAsync().ConfigureAwait(false);
            return result.Select(r => r.ToDomain());
        }

        public async Task<bool> Update(CredentialSetting passwordSettings)
        {
            var record = await _context.CredentialSettings.FirstOrDefaultAsync().ConfigureAwait(false);
            if (record == null)
            {
                _context.CredentialSettings.Add(new Models.CredentialSetting
                {
                    AuthenticationIntervalsInSeconds = passwordSettings.AuthenticationIntervalsInSeconds,
                    IsBlockAccountPolicyEnabled = passwordSettings.IsBlockAccountPolicyEnabled,
                    NumberOfAuthenticationAttempts = passwordSettings.NumberOfAuthenticationAttempts,
                    CredentialType = passwordSettings.CredentialType,
                    ExpiresIn = passwordSettings.ExpiresIn,
                    Options = passwordSettings.Options
                });
            }
            else
            {
                record.AuthenticationIntervalsInSeconds = passwordSettings.AuthenticationIntervalsInSeconds;
                record.IsBlockAccountPolicyEnabled = passwordSettings.IsBlockAccountPolicyEnabled;
                record.NumberOfAuthenticationAttempts = passwordSettings.NumberOfAuthenticationAttempts;
                record.ExpiresIn = passwordSettings.ExpiresIn;
                record.Options = passwordSettings.Options;
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
