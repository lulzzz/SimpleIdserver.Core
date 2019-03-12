using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.Repositories
{
    public interface IUserRepository
    {
        Task<bool> Authenticate(string login, string password);
        Task<User> GetUserByClaim(string key, string value);
        Task<User> Get(string id);
        Task<ICollection<User>> Get(IEnumerable<System.Security.Claims.Claim> claims);
        Task<ICollection<User>> GetAll();
        Task<bool> InsertAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(string subject);
        Task<SearchUserResult> Search(SearchUserParameter parameter);
    }
}