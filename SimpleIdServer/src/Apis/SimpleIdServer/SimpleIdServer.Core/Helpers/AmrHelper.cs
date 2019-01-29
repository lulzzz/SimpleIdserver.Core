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
        Task<string> GetNextAmr(string actualAcr, string actualAmr);
    }

    internal sealed class AmrHelper : IAmrHelper
    {
        private readonly IAuthenticationContextclassReferenceRepository _authenticationContextclassReferenceRepository;

        public AmrHelper(IAuthenticationContextclassReferenceRepository authenticationContextclassReferenceRepository)
        {
            _authenticationContextclassReferenceRepository = authenticationContextclassReferenceRepository;
        }

        public async Task<string> GetNextAmr(string actualAcr, string actualAmr)
        {
            if (string.IsNullOrWhiteSpace(actualAcr))
            {
                throw new ArgumentNullException(nameof(actualAcr));
            }

            if (string.IsNullOrWhiteSpace(actualAmr))
            {
                throw new ArgumentNullException(nameof(actualAmr));
            }

            var acr = await _authenticationContextclassReferenceRepository.Get(actualAcr).ConfigureAwait(false);
            if(acr == null)
            {
                return null;
            }

            var index = acr.AmrLst.ToList().IndexOf(actualAmr);
            if (index + 1 < acr.AmrLst.Count())
            {
                return acr.AmrLst.ElementAt(index + 1);
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
