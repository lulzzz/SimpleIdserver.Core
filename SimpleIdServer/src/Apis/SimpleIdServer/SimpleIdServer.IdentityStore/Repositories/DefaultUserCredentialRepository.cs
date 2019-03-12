using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    internal sealed class DefaultUserCredentialRepository : IUserCredentialRepository
    {
        public ICollection<User> _users;

        public DefaultUserCredentialRepository(ICollection<User> users)
        {
            _users = users;
        }

        public Task<bool> Add(IEnumerable<UserCredential> resourceOwnerCredentials)
        {
            if (resourceOwnerCredentials == null)
            {
                return Task.FromResult(true);
            }

            foreach (var resourceOwnerCredential in resourceOwnerCredentials)
            {
                var user = _users.FirstOrDefault(u => u.Id == resourceOwnerCredential.UserId);
                if (user == null)
                {
                    continue;
                }

                var credentials = user.Credentials == null ? new List<UserCredential>() : user.Credentials.ToList();
                credentials.Add(new UserCredential
                {
                    BlockedDateTime = resourceOwnerCredential.BlockedDateTime,
                    ExpirationDateTime = resourceOwnerCredential.ExpirationDateTime,
                    FirstAuthenticationFailureDateTime = resourceOwnerCredential.FirstAuthenticationFailureDateTime,
                    IsBlocked = resourceOwnerCredential.IsBlocked,
                    NumberOfAttempts = resourceOwnerCredential.NumberOfAttempts,
                    Type = resourceOwnerCredential.Type,
                    UserId = resourceOwnerCredential.UserId,
                    Value = resourceOwnerCredential.Value
                });
                user.Credentials = credentials;
            }

            return Task.FromResult(true);
        }

        public Task<bool> Delete(string subject, string type)
        {
            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            var creds = user.Credentials.ToList();
            var credential = creds.FirstOrDefault(c => c.Type == type);
            if (credential == null)
            {
                return Task.FromResult(false);
            }

            creds.Remove(credential);
            user.Credentials = creds;
            return Task.FromResult(true);
        }

        public Task<UserCredential> Get(string type, string value)
        {
            foreach (var user in _users)
            {
                if (user.Credentials == null)
                {
                    continue;
                }

                var credential = user.Credentials.FirstOrDefault(c => c.Type == type && c.Value == value);
                if (credential != null)
                {
                    return Task.FromResult(credential);
                }
            }

            return Task.FromResult((UserCredential)null);
        }

        public Task<UserCredential> GetUserCredential(string subject, string type)
        {
            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult((UserCredential)null);
            }

            var credential = user.Credentials.FirstOrDefault(c => c.Type == type);
            return Task.FromResult(credential);
        }

        public async Task<bool> Update(UserCredential resourceOwnerCredential)
        {
            var cred = await GetUserCredential(resourceOwnerCredential.UserId, resourceOwnerCredential.Type).ConfigureAwait(false);
            if (cred == null)
            {
                return false;
            }

            cred.BlockedDateTime = resourceOwnerCredential.BlockedDateTime;
            cred.ExpirationDateTime = resourceOwnerCredential.ExpirationDateTime;
            cred.FirstAuthenticationFailureDateTime = resourceOwnerCredential.FirstAuthenticationFailureDateTime;
            cred.IsBlocked = resourceOwnerCredential.IsBlocked;
            cred.NumberOfAttempts = resourceOwnerCredential.NumberOfAttempts;
            cred.Value = resourceOwnerCredential.Value;
            return true;
        }
    }
}
