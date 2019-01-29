using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Repositories
{
    public class DefaultAuthenticationContextclassReferenceRepository : IAuthenticationContextclassReferenceRepository
    {
        private ICollection<AuthenticationContextclassReference> _acrLst;

        public DefaultAuthenticationContextclassReferenceRepository(ICollection<AuthenticationContextclassReference> acrLst)
        {
            _acrLst = acrLst == null ? new List<AuthenticationContextclassReference>() : acrLst;
        }

        public Task<IEnumerable<AuthenticationContextclassReference>> Get()
        {
            return Task.FromResult((IEnumerable<AuthenticationContextclassReference>)_acrLst);
        }

        public Task<AuthenticationContextclassReference> Get(string name)
        {
            return Task.FromResult(_acrLst.FirstOrDefault(a => a.Name == name));
        }

        public Task<IEnumerable<AuthenticationContextclassReference>> Get(IEnumerable<string> names)
        {
            return Task.FromResult(_acrLst.Where(a => names.Contains(a.Name)));
        }
    }
}
