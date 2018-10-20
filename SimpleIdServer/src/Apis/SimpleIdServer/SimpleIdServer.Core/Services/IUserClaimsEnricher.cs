using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public interface IUserClaimsEnricher
    {
        Task Enrich(List<Claim> claims);
    }
}
