using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;

namespace SimpleIdServer.Core.Helpers
{
    public interface IAmrHelper
    {
        string GetAmr(IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs);
        Task<string> GetNextAmr(string actualAcr, IEnumerable<string> actualAmrs);
    }

    internal sealed class AmrHelper : IAmrHelper
    {
        private readonly IAuthenticationContextclassReferenceRepository _authenticationContextclassReferenceRepository;

        public AmrHelper(IAuthenticationContextclassReferenceRepository authenticationContextclassReferenceRepository)
        {
            _authenticationContextclassReferenceRepository = authenticationContextclassReferenceRepository;
        }

        public async Task<string> GetNextAmr(string actualAcr, IEnumerable<string> actualAmrs)
        {
            if (string.IsNullOrWhiteSpace(actualAcr))
            {
                throw new ArgumentNullException(nameof(actualAcr));
            }

            if (actualAmrs == null)
            {
                throw new ArgumentNullException(nameof(actualAmrs));
            }

            var acr = await _authenticationContextclassReferenceRepository.Get(actualAcr).ConfigureAwait(false);
            if (acr == null)
            {
                return null;
            }

            if (actualAmrs.Count() < acr.AmrLst.Count())
            {
                return acr.AmrLst.ElementAt(actualAmrs.Count());
            }

            return null;
        }

        public string GetAmr(IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs)
        {
            if (currentAmrs == null || !currentAmrs.Any())
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NoActiveAmr);
            }

            var amr = Constants.DEFAULT_AMR;
            if (exceptedAmrs != null)
            {
                foreach(var exceptedAmr in exceptedAmrs)
                {
                    if (currentAmrs.Contains(exceptedAmr))
                    {
                        amr = exceptedAmr;
                        break;
                    }
                }
            }

            if (!currentAmrs.Contains(amr))
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.TheAmrDoesntExist, amr));
            }

            return amr;
        }
    }
}
