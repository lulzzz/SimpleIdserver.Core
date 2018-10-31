using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public class DefaultClientInfoService : IClientInfoService
    {
        public Task<string> GetClientId()
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> GetClientSecret()
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}