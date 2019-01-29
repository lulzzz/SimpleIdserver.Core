using SimpleIdServer.Core.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Common.Repositories
{
    public interface IAuthenticationContextclassReferenceRepository
    {
        Task<IEnumerable<AuthenticationContextclassReference>> Get();
        Task<AuthenticationContextclassReference> Get(string name);
        Task<AuthenticationContextclassReference> GetDefault();
        Task<IEnumerable<AuthenticationContextclassReference>> Get(IEnumerable<string> names);
    }
}