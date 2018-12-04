using SimpleIdServer.Uma.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public class DefaultSharedLinkRepository : ISharedLinkRepository
    {
        public ICollection<SharedLink> _sharedLinks = null;
        private static object _obj = new object();

        public DefaultSharedLinkRepository(ICollection<SharedLink> sharedLinks)
        {
            _sharedLinks = sharedLinks == null ? new List<SharedLink>() : sharedLinks;
        }

        public Task<bool> Delete(string confirmationCode)
        {
            lock(_obj)
            {
                var record = _sharedLinks.FirstOrDefault(s => s.ConfirmationCode == confirmationCode);
                if (record == null)
                {
                    return Task.FromResult(false);
                }

                _sharedLinks.Remove(record);
            }

            return Task.FromResult(true);
        }

        public Task<SharedLink> Get(string confirmationCode)
        {
            return Task.FromResult(_sharedLinks.FirstOrDefault(s => s.ConfirmationCode == confirmationCode));
        }

        public Task<bool> Insert(SharedLink sharedLink)
        {
            _sharedLinks.Add(new SharedLink
            {
                ConfirmationCode = sharedLink.ConfirmationCode,
                ResourceId = sharedLink.ResourceId,
                Scopes = sharedLink.Scopes
            });
            return Task.FromResult(true);
        }
    }
}
