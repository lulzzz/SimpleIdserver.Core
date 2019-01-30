using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IRemoveConsentOperation
    {
        Task<bool> Execute(string consentId);
    }

    internal class RemoveConsentOperation : IRemoveConsentOperation
    {
        private readonly IConsentRepository _consentRepository;
        
        public RemoveConsentOperation(IConsentRepository consentRepository)
        {
            _consentRepository = consentRepository;
        }
        
        public async Task<bool> Execute(string consentId)
        {
            if (string.IsNullOrWhiteSpace(consentId))
            {
                throw new ArgumentNullException(consentId);
            }

            var consentToBeDeleted = new Common.Models.Consent
            {
                Id = consentId
            };

            return await _consentRepository.DeleteAsync(consentToBeDeleted);
        }
    }
}
