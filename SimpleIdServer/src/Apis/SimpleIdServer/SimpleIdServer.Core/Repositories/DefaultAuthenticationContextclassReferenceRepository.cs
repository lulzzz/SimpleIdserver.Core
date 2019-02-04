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

        public Task<bool> Add(AuthenticationContextclassReference acr)
        {
            _acrLst.Add(new AuthenticationContextclassReference
            {
                AmrLst = acr.AmrLst,
                DisplayName = acr.DisplayName,
                IsDefault = false,
                Name = acr.Name,
                Type = acr.Type
            });
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string name)
        {
            var acr = _acrLst.FirstOrDefault(a => a.Name == name);
            if (acr == null)
            {
                return Task.FromResult(false);
            }

            _acrLst.Remove(acr);
            return Task.FromResult(true);
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

        public Task<AuthenticationContextclassReference> GetDefault()
        {
            return Task.FromResult(_acrLst.FirstOrDefault(a => a.IsDefault));
        }

        public Task<bool> Update(AuthenticationContextclassReference acr)
        {
            var rec = _acrLst.FirstOrDefault(a => a.Name == acr.Name);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.AmrLst = acr.AmrLst;
            rec.DisplayName = acr.DisplayName;
            rec.Type = acr.Type;
            return Task.FromResult(true);
        }
    }
}
