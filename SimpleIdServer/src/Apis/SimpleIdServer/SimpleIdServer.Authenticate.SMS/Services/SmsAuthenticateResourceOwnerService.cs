using SimpleIdServer.Core.Services;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.Store;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.SMS.Services
{
    internal sealed class SmsAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly IUserRepository _userRepository;

        public SmsAuthenticateResourceOwnerService(IConfirmationCodeStore confirmationCodeStore, IUserRepository userRepository)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _userRepository = userRepository;
        }

        public string Amr => Constants.AMR;

        public async Task<User> AuthenticateUserAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var resourceOwner = await _userRepository.GetUserByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, login).ConfigureAwait(false);
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
