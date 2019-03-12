using SimpleIdServer.IdentityStore.Models;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public interface IAuthenticateResourceOwnerService
    {
        Task<User> AuthenticateUserAsync(string login, string password);
        string Amr { get; }
    }
}
