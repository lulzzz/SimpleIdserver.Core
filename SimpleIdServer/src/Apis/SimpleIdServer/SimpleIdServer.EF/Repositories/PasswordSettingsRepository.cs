using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
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
    }
}
