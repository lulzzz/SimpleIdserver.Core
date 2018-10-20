using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.TwoFactorAuthentication
{
    public interface ITwoFactorAuthenticationService
    {
        Task SendAsync(string code, ResourceOwner user);
        string RequiredClaim { get; }
        string Name { get; }
    }
}
