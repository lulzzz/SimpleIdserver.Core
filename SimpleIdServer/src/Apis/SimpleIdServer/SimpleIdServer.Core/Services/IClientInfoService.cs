using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public interface IClientInfoService
    {
        Task<string> GetClientId();
        Task<string> GetClientSecret();
    }
}
