using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.IdentityStore.Results;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.EF.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly IdentityStoreEFContext _context;

        public UserRepository(IdentityStoreEFContext context)
        {
            _context = context;
        }

        public Task<bool> DeleteAsync(string subject)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> Get(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<ICollection<User>> Get(IEnumerable<Claim> claims)
        {
            throw new System.NotImplementedException();
        }

        public Task<ICollection<User>> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public Task<User> GetUserByClaim(string key, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> InsertAsync(User user)
        {
            throw new System.NotImplementedException();
        }

        public Task<SearchUserResult> Search(SearchUserParameter parameter)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UpdateAsync(User user)
        {
            throw new System.NotImplementedException();
        }

        Task<User> IUserRepository.Get(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
