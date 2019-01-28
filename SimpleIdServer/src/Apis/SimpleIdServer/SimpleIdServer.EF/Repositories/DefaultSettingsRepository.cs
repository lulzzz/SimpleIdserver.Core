using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
using System.Threading.Tasks;

namespace SimpleIdServer.EF.Repositories
{
    internal sealed class DefaultSettingsRepository : IDefaultSettingsRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public DefaultSettingsRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        public async Task<DefaultSettings> Get()
        {
            var result = await _context.DefaultSettings.FirstOrDefaultAsync().ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }
    }
}
