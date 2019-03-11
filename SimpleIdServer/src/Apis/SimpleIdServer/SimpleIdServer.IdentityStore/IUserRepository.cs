using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore
{
    public interface IUserRepository
    {
        Task<User> Get(string id);
    }
}