using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;

namespace SimpleIdServer.IdentityStore.EF.Repositories
{
    internal sealed class UserCredentialRepository : IUserCredentialRepository
    {
        private readonly IdentityStoreEFContext _context;

        public UserCredentialRepository(IdentityStoreEFContext context)
        {
            _context = context;
        }

        public async Task<bool> Add(IEnumerable<UserCredential> userCredentials)
        {
            foreach (var userCredential in userCredentials)
            {
                _context.UserCredentials.Add(new Models.UserCredential
                {
                    BlockedDateTime = userCredential.BlockedDateTime,
                    ExpirationDateTime = userCredential.ExpirationDateTime,
                    FirstAuthenticationFailureDateTime = userCredential.FirstAuthenticationFailureDateTime,
                    IsBlocked = userCredential.IsBlocked,
                    NumberOfAttempts = userCredential.NumberOfAttempts,
                    UserId = userCredential.UserId,
                    Type = userCredential.Type,
                    Value = userCredential.Value
                });
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Delete(string subject, string type)
        {
            var record = await _context.UserCredentials.FirstOrDefaultAsync(c => c.Type == type && c.UserId == subject).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.UserCredentials.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<UserCredential> Get(string type, string value)
        {
            var record = await _context.UserCredentials.FirstOrDefaultAsync(c => c.Type == type && c.Value == value).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return new UserCredential
            {
                BlockedDateTime = record.BlockedDateTime,
                ExpirationDateTime = record.ExpirationDateTime,
                FirstAuthenticationFailureDateTime = record.FirstAuthenticationFailureDateTime,
                IsBlocked = record.IsBlocked,
                NumberOfAttempts = record.NumberOfAttempts,
                Type = record.Type,
                UserId = record.UserId,
                Value = record.Value
            };
        }

        public async Task<UserCredential> GetUserCredential(string subject, string type)
        {
            var record = await _context.UserCredentials.FirstOrDefaultAsync(c => c.Type == type && c.UserId == subject).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return new UserCredential
            {
                BlockedDateTime = record.BlockedDateTime,
                ExpirationDateTime = record.ExpirationDateTime,
                FirstAuthenticationFailureDateTime = record.FirstAuthenticationFailureDateTime,
                IsBlocked = record.IsBlocked,
                NumberOfAttempts = record.NumberOfAttempts,
                Type = record.Type,
                UserId = record.UserId,
                Value = record.Value
            };
        }

        public async Task<bool> Update(UserCredential resourceOwnerCredential)
        {
            var record = await _context.UserCredentials.FirstOrDefaultAsync(c => c.UserId == resourceOwnerCredential.UserId && c.Type == resourceOwnerCredential.Type).ConfigureAwait(false);
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
