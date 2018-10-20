using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;

namespace SimpleIdServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserTwoFactorAuthenticatorOperation
    {
        Task<bool> Execute(string subject, string twoFactorAuth);
    }

    internal sealed class UpdateUserTwoFactorAuthenticatorOperation : IUpdateUserTwoFactorAuthenticatorOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserTwoFactorAuthenticatorOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string twoFactorAuth)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.GetAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.TwoFactorAuthentication = twoFactorAuth;
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner);
        }
    }
}
