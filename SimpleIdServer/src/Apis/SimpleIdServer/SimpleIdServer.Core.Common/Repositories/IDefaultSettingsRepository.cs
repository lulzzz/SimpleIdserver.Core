using SimpleIdServer.Core.Common.Models;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface IDefaultSettingsRepository
    {
        Task<DefaultSettings> Get();
    }
}
