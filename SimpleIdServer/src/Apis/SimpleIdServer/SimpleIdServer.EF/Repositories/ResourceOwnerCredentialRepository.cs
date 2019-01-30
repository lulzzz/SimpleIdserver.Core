using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<ResourceOwnerCredential> Get(string type, string value)
        {
            var record = await _context.ResourceOwnerCredentials.FirstOrDefaultAsync(c => c.Type == type && c.Value == value).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return new ResourceOwnerCredential
            {
                BlockedDateTime = record.BlockedDateTime,
                ExpirationDateTime = record.ExpirationDateTime,
                FirstAuthenticationFailureDateTime = record.FirstAuthenticationFailureDateTime,
                IsBlocked = record.IsBlocked,
                NumberOfAttempts = record.NumberOfAttempts,
                Type = record.Type,
                UserId = record.ResourceOwnerId,
                Value = record.Value
            };
        }

        public async Task<ResourceOwnerCredential> GetUserCredential(string subject, string type)
        {
            var record = await _context.ResourceOwnerCredentials.FirstOrDefaultAsync(c => c.Type == type && c.ResourceOwnerId == subject).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return new ResourceOwnerCredential
            {
                BlockedDateTime = record.BlockedDateTime,
                ExpirationDateTime = record.ExpirationDateTime,
                FirstAuthenticationFailureDateTime = record.FirstAuthenticationFailureDateTime,
                IsBlocked = record.IsBlocked,
                NumberOfAttempts = record.NumberOfAttempts,
                Type = record.Type,
                UserId = record.ResourceOwnerId,
                Value = record.Value
            };
        }

        public async Task<bool> Update(ResourceOwnerCredential resourceOwnerCredential)
        {
            var record = await _context.ResourceOwnerCredentials.FirstOrDefaultAsync(c => c.ResourceOwnerId == resourceOwnerCredential.UserId && c.Type == resourceOwnerCredential.Type).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            record.BlockedDateTime = resourceOwnerCredential.BlockedDateTime;
            record.ExpirationDateTime = resourceOwnerCredential.ExpirationDateTime;
            record.FirstAuthenticationFailureDateTime = resourceOwnerCredential.FirstAuthenticationFailureDateTime;
            record.IsBlocked = resourceOwnerCredential.IsBlocked;
            record.NumberOfAttempts = resourceOwnerCredential.NumberOfAttempts;
            record.Value = resourceOwnerCredential.Value;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
