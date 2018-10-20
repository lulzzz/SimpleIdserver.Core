using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.Store
{
    public interface IAuthorizationCodeStore
    {
        Task<AuthorizationCode> GetAuthorizationCode(string code);
        Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode);
        Task<bool> RemoveAuthorizationCode(string code);
    }
}
