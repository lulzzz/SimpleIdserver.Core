using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Store;

namespace SimpleIdServer.Authenticate.SMS.Services
{
    internal sealed class SmsAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public SmsAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, IConfirmationCodeStore confirmationCodeStore)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeStore = confirmationCodeStore;
        }

        public string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public async Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var confirmationCode = await _confirmationCodeStore.Get(password);
            if (confirmationCode == null || confirmationCode.Subject != login)
            {
                return null;
            }

            if (confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn) <= DateTime.UtcNow)
            {
                return null;
            }

            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, login);
            if (resourceOwner != null)
            {
                await _confirmationCodeStore.Remove(password);
            }

            return resourceOwner;
        }
    }
}
