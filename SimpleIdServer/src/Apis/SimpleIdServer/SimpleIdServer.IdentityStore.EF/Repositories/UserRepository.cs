using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.IdentityStore.EF.Extensions;
using SimpleIdServer.IdentityStore.EF.Models;
using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.IdentityStore.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains = SimpleIdServer.IdentityStore.Models;

namespace SimpleIdServer.IdentityStore.EF.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly IdentityStoreEFContext _context;

        public UserRepository(IdentityStoreEFContext context)
        {
            _context = context;
        }

        public async Task<bool> Authenticate(string login, string password)
        {
            var result = await _context.Users
                .Include(r => r.Credentials)
                .FirstOrDefaultAsync(r => r.Id == login)
                .ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            var pwdCred = result.Credentials.FirstOrDefault(c => c.Type == "pwd");
            if (pwdCred == null)
            {
                return false;
            }

            return pwdCred.Value == PasswordHelper.ComputeHash(password);
        }

        public async Task<Domains.User> GetUserByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var result = await _context.Users.Include(r => r.Claims).Include(r => r.Credentials)
                .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == key && c.Value == value))
                .ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<Domains.User> Get(string id)
        {
            var result = await _context.Users
                .Include(r => r.Claims)
                .Include(r => r.Credentials)
                .FirstOrDefaultAsync(r => r.Id == id)
                .ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<ICollection<Domains.User>> Get(IEnumerable<System.Security.Claims.Claim> claims)
        {
            if (claims == null)
            {
                return new List<Domains.User>();
            }

            return await _context.Users
                .Include(r => r.Claims)
                .Include(r => r.Credentials)
                .Where(r => claims.All(c => r.Claims.Any(sc => sc.Value == c.Value && sc.ClaimCode == c.Type)))
                .Select(u => u.ToDomain())
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<ICollection<Domains.User>> GetAll()
        {
            return await _context.Users.Select(u => u.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var record = await _context.Users
               .Include(r => r.Claims)
               .Include(r => r.Credentials)
               .FirstOrDefaultAsync(r => r.Id == id);
            if (record == null)
            {
                return false;
            }

            _context.Users.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> InsertAsync(Domains.User user)
        {
            var newUser = new User
            {
                Id = user.Id,
                IsBlocked = user.IsBlocked,
                Claims = new List<UserClaim>(),
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            };

            if (user.Claims != null)
            {
                foreach (var claim in user.Claims)
                {
                    newUser.Claims.Add(new UserClaim
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        ClaimCode = claim.Type,
                        Value = claim.Value
                    });
                }
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> UpdateAsync(Domains.User user)
        {
            var record = await _context.Users
                .Include(r => r.Claims)
                .FirstOrDefaultAsync(r => r.Id == user.Id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            record.IsBlocked = user.IsBlocked;
            record.UpdateDateTime = DateTime.UtcNow;
            record.Claims = new List<UserClaim>();
            if (user.Claims != null)
            {
                foreach (var claim in user.Claims)
                {
                    record.Claims.Add(new UserClaim
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = record.Id,
                        ClaimCode = claim.Type,
                        Value = claim.Value
                    });
                }
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<SearchUserResult> Search(SearchUserParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<User> result = _context.Users
                .Include(r => r.Claims)
                .Include(r => r.Credentials);

            if (parameter.Subjects != null)
            {
                result = result.Where(r => parameter.Subjects.Contains(r.Id));
            }

            if (result == null)
            {
                return null;
            }

            var nbResult = await result.CountAsync().ConfigureAwait(false);
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

            return new SearchUserResult
            {
                Content = result.Select(r => r.ToDomain()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            };
        }
    }
}
