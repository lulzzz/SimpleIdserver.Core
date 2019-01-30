using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetConsentsOperation
    {
        Task<IEnumerable<Common.Models.Consent>> Execute(string subject);
    }

    internal class GetConsentsOperation : IGetConsentsOperation
    {
        private readonly IConsentRepository _consentRepository;

        public GetConsentsOperation(IConsentRepository consentRepository)
        {
            _consentRepository = consentRepository;
        }
        
        public Task<IEnumerable<Common.Models.Consent>> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return _consentRepository.GetConsentsForGivenUserAsync(subject);
        }
    }
}
