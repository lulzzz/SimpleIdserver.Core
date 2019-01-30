using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Store;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.SMS.Services
{
    internal sealed class SmsAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public SmsAuthenticateResourceOwnerService(IConfirmationCodeStore confirmationCodeStore, IResourceOwnerRepository resourceOwnerRepository)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public string Amr => Constants.AMR;

        public async Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return null;
            }

            var confirmationCode = await _confirmationCodeStore.Get(password).ConfigureAwait(false);
            if (confirmationCode == null || confirmationCode.Subject != resourceOwner.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber).Value)
            {
                return null;
            }

            if (confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn) <= DateTime.UtcNow)
            {
                return null;
            }

            await _confirmationCodeStore.Remove(password).ConfigureAwait(false);
            return resourceOwner;
        }
    }
}
