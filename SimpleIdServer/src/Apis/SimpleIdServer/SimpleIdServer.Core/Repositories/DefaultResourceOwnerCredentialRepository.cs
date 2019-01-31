using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Repositories
{
    internal sealed class DefaultResourceOwnerCredentialRepository : IResourceOwnerCredentialRepository
    {
        public ICollection<ResourceOwner> _users;

        public DefaultResourceOwnerCredentialRepository(ICollection<ResourceOwner> users)
        {
            _users = users;
        }

        public Task<bool> Add(IEnumerable<ResourceOwnerCredential> resourceOwnerCredentials)
        {
            if (resourceOwnerCredentials == null)
            {
                return Task.FromResult(true);
            }

            foreach(var resourceOwnerCredential in resourceOwnerCredentials)
            {
                var user = _users.FirstOrDefault(u => u.Id == resourceOwnerCredential.UserId);
                if (user == null)
                {
                    continue;
                }

                var credentials = user.Credentials == null ? new List<ResourceOwnerCredential>() : user.Credentials.ToList();
                credentials.Add(new ResourceOwnerCredential
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

        public Task<ResourceOwnerCredential> Get(string type, string value)
        {
            foreach(var user in _users)
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

            return Task.FromResult((ResourceOwnerCredential)null);
        }

        public Task<ResourceOwnerCredential> GetUserCredential(string subject, string type)
        {
            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult((ResourceOwnerCredential)null);
            }
            
            var credential = user.Credentials.FirstOrDefault(c => c.Type == type);
            return Task.FromResult(credential);
        }

        public async Task<bool> Update(ResourceOwnerCredential resourceOwnerCredential)
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