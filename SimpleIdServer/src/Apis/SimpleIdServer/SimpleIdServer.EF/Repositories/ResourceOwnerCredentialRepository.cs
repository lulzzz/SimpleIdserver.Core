using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;

namespace SimpleIdServer.EF.Repositories
{
    internal sealed class ResourceOwnerCredentialRepository : IResourceOwnerCredentialRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public ResourceOwnerCredentialRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        public async Task<bool> Add(IEnumerable<ResourceOwnerCredential> resourceOwnerCredentials)
        {
            foreach(var resourceOwnerCredential in resourceOwnerCredentials)
            {
                _context.ResourceOwnerCredentials.Add(new Models.ResourceOwnerCredential
                {
                    BlockedDateTime = resourceOwnerCredential.BlockedDateTime,
                    ExpirationDateTime = resourceOwnerCredential.ExpirationDateTime,
                    FirstAuthenticationFailureDateTime = resourceOwnerCredential.FirstAuthenticationFailureDateTime,
                    IsBlocked = resourceOwnerCredential.IsBlocked,
                    NumberOfAttempts = resourceOwnerCredential.NumberOfAttempts,
                    ResourceOwnerId = resourceOwnerCredential.UserId,
                    Type = resourceOwnerCredential.Type,
                    Value = resourceOwnerCredential.Value
                });
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public Task<ResourceOwnerCredential> Get(string type, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResourceOwnerCredential> GetUserCredential(string subject, string type)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(ResourceOwnerCredential resourceOwnerCredential)
        {
            throw new System.NotImplementedException();
        }
    }
}
