using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Store;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.SMS.Services
{
    internal sealed class SmsAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public SmsAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, IConfirmationCodeStore confirmationCodeStore,
            ICredentialSettingsRepository passwordSettingsRepository) : base(passwordSettingsRepository, resourceOwnerRepository)
        {
            _confirmationCodeStore = confirmationCodeStore;
        }

        public override string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public override async Task<bool> Authenticate(ResourceOwner user, string password)
        {
            var confirmationCode = await _confirmationCodeStore.Get(password).ConfigureAwait(false);
            if (confirmationCode == null || confirmationCode.Subject != user.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber).Value)
            {
                return false;
            }

            if (confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn) <= DateTime.UtcNow)
            {
                return false;
            }

            await _confirmationCodeStore.Remove(password).ConfigureAwait(false);
            return true;
        }

        public override Task<ResourceOwner> GetResourceOwner(string login)
        {
            return ResourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, login);
        }

        public override Task Validate(ResourceOwner user)
        {
            return Task.FromResult(0);
        }
    }
}
