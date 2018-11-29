using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.EF.Extensions;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.EF.Repositories
{
    internal sealed class SharedLinkRepository : ISharedLinkRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public SharedLinkRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<bool> Delete(string confirmationCode)
        {
            var record = await _context.ShareResourceLinks.FirstOrDefaultAsync(s => s.ConfirmationCode == confirmationCode).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.ShareResourceLinks.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<SharedLink> Get(string confirmationCode)
        {
            var record = await _context.ShareResourceLinks.FirstOrDefaultAsync(s => s.ConfirmationCode == confirmationCode).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return record.ToDomain();
        }

        public async Task<bool> Insert(SharedLink sharedLink)
        {
            await _context.ShareResourceLinks.AddAsync(sharedLink.ToModel()).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
