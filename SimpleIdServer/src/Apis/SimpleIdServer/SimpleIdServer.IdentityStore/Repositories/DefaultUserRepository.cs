using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.IdentityStore.Extensions;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    internal sealed class DefaultUserRepository : IUserRepository
    {
        public ICollection<User> _users;

        public DefaultUserRepository(ICollection<User> users)
        {
            _users = users == null ? new List<User>() : users;
        }

        public Task<bool> Authenticate(string login, string password)
        {
            var user = _users.FirstOrDefault(u => u.Id == login);
            var pwdCred = user.Credentials.FirstOrDefault(c => c.Type == "pwd");
            if (pwdCred == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(pwdCred.Value == PasswordHelper.ComputeHash(password));
        }

        public Task<bool> DeleteAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            _users.Remove(user);
            return Task.FromResult(true);
        }

        public Task<ICollection<User>> GetAll()
        {
            ICollection<User> res = _users.Select(u => u.Copy()).ToList();
            return Task.FromResult(res);
        }

        public Task<User> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Task.FromResult((User)null);
            }

            return Task.FromResult(user.Copy());
        }

        public Task<ICollection<User>> Get(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            ICollection<User> result = _users.Where(u => claims.All(c => u.Claims.Any(sc => sc.Value == c.Value && sc.Type == c.Type)))
                .Select(u => u.Copy())
                .ToList();
            return Task.FromResult(result);
        }

        public Task<User> GetUserByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var user = _users.FirstOrDefault(u => u.Claims.Any(c => c.Type == key && c.Value == value));
            if (user == null)
            {
                return Task.FromResult((User)null);
            }

            return Task.FromResult(user.Copy());
        }

        public Task<bool> InsertAsync(User resourceOwner)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            resourceOwner.CreateDateTime = DateTime.UtcNow;
            _users.Add(resourceOwner.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchUserResult> Search(SearchUserParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<User> result = _users;
            if (parameter.Subjects != null)
            {
                result = result.Where(r => parameter.Subjects.Any(s => r.Id.Contains(s)));
            }

            var nbResult = result.Count();
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                result = result.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                result = result.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                result = result.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchUserResult
            {
                Content = result.Select(u => u.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> UpdateAsync(User resourceOwner)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var user = _users.FirstOrDefault(u => u.Id == resourceOwner.Id);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            user.IsBlocked = resourceOwner.IsBlocked;
            user.UpdateDateTime = DateTime.UtcNow;
            user.Claims = resourceOwner.Claims;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateCredential(string subject, UserCredential credential)
        {
            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            var cr = user.Credentials.FirstOrDefault(c => c.Type == credential.Type);
            if (cr == null)
            {
                cr = new UserCredential();
            }

            cr.Type = credential.Type;
            cr.Value = credential.Value;
            cr.IsBlocked = credential.IsBlocked;
            cr.NumberOfAttempts = credential.NumberOfAttempts;
            cr.BlockedDateTime = credential.BlockedDateTime;
            cr.ExpirationDateTime = credential.ExpirationDateTime;
            cr.FirstAuthenticationFailureDateTime = credential.FirstAuthenticationFailureDateTime;
            return Task.FromResult(true);
        }
    }
}
