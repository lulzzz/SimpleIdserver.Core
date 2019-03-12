using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Profile.Actions
{
    public interface ILinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject, string issuer, bool force = false);
    }

    internal sealed class LinkProfileAction : ILinkProfileAction
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;

        public LinkProfileAction(IUserRepository userRepository, IProfileRepository profileRepository)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
        }

        public async Task<bool> Execute(string localSubject, string externalSubject, string issuer, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(localSubject))
            {
                throw new ArgumentNullException(nameof(localSubject));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            var resourceOwner = await _userRepository.Get(localSubject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            var profile = await _profileRepository.Get(externalSubject);
            if (profile != null && profile.UserId != localSubject)
            {
                if (!force)
                {
                    throw new ProfileAssignedAnotherAccountException();
                }
                else
                {
                    await _profileRepository.Remove(new[] { externalSubject });
                    if (profile.UserId == profile.Subject)
                    {
                        await _userRepository.DeleteAsync(profile.UserId).ConfigureAwait(false);
                    }

                    profile = null;
                }
            }

            if (profile != null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheProfileAlreadyLinked);
            }

            return await _profileRepository.Add(new[]
            {
                new ResourceOwnerProfile
                {
                    UserId = localSubject,
                    Subject = externalSubject,
                    Issuer = issuer,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                }
            });
        }
    }
}
