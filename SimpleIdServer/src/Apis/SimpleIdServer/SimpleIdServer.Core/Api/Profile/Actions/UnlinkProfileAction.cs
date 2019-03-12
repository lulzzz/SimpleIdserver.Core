using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.IdentityStore.Repositories;

namespace SimpleIdServer.Core.Api.Profile.Actions
{
    public interface IUnlinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject);
    }

    internal sealed class UnlinkProfileAction : IUnlinkProfileAction
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;
        
        public UnlinkProfileAction(IUserRepository userRepository, IProfileRepository profileRepository)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
        }

        public async Task<bool> Execute(string localSubject, string externalSubject)
        {
            if (string.IsNullOrWhiteSpace(localSubject))
            {
                throw new ArgumentNullException(nameof(localSubject));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }
            
            var resourceOwner = await _userRepository.Get(localSubject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }
            
            var profile = await _profileRepository.Get(externalSubject);
            if (profile == null || profile.UserId != localSubject)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NotAuthorizedToRemoveTheProfile);
            }

            if (profile.Subject == localSubject)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheExternalAccountAccountCannotBeUnlinked);
            }

            return await _profileRepository.Remove(new[] { externalSubject });
        }
    }
}
