using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.EF.Repositories
{
    internal sealed class PasswordSettingsRepository : IPasswordSettingsRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public PasswordSettingsRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        public async Task<PasswordSettings> Get()
        {
            var result = await _context.PasswordSettings.FirstOrDefaultAsync().ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<bool> Update(PasswordSettings passwordSettings)
        {
            var record = await _context.PasswordSettings.FirstOrDefaultAsync().ConfigureAwait(false);
            if (record == null)
            {
                _context.PasswordSettings.Add(new Models.PasswordSettings
                {
                    AuthenticationIntervalsInSeconds = passwordSettings.AuthenticationIntervalsInSeconds,
                    Id = Guid.NewGuid().ToString(),
                    IsBlockAccountPolicyEnabled = passwordSettings.IsBlockAccountPolicyEnabled,
                    IsRegexEnabled = passwordSettings.IsRegexEnabled,
                    NumberOfAuthenticationAttempts = passwordSettings.NumberOfAuthenticationAttempts,
                    PasswordDescription = passwordSettings.PasswordDescription,
                    PasswordExpiresIn = passwordSettings.PasswordExpiresIn,
                    RegularExpression = passwordSettings.RegularExpression
                });
            }
            else
            {
                record.AuthenticationIntervalsInSeconds = passwordSettings.AuthenticationIntervalsInSeconds;
                record.IsBlockAccountPolicyEnabled = passwordSettings.IsBlockAccountPolicyEnabled;
                record.IsRegexEnabled = passwordSettings.IsRegexEnabled;
                record.NumberOfAuthenticationAttempts = passwordSettings.NumberOfAuthenticationAttempts;
                record.PasswordDescription = passwordSettings.PasswordDescription;
                record.PasswordExpiresIn = passwordSettings.PasswordExpiresIn;
                record.RegularExpression = passwordSettings.RegularExpression;
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
