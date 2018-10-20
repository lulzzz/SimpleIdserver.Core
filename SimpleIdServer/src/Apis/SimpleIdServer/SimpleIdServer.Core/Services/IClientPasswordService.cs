
namespace SimpleIdServer.Core.Services
{
    public interface IClientPasswordService
    {
        string Encrypt(string password);
    }
}
