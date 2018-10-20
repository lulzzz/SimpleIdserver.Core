using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;

namespace SimpleIdServer.Core.Api.Profile.Actions
{
    public interface ILinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject, string issuer, bool force = false);
    }

    internal sealed class LinkProfileAction : ILinkProfileAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IProfileRepository _profileRepository;

        public LinkProfileAction(IResourceOwnerRepository resourceOwnerRepository, IProfileRepository profileRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
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

            var resourceOwner = await _resourceOwnerRepository.GetAsync(localSubject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            var profile = await _profileRepository.Get(externalSubject);
            if (profile != null && profile.ResourceOwnerId != localSubject)
            {
                if (!force)
                {
                    throw new ProfileAssignedAnotherAccountException();
                }
                else
                {
                    await _profileRepository.Remove(new[] { externalSubject });
                    if (profile.ResourceOwnerId == profile.Subject)
                    {
                        await _resourceOwnerRepository.DeleteAsync(profile.ResourceOwnerId);
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
                    ResourceOwnerId = localSubject,
                    Subject = externalSubject,
                    Issuer = issuer,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                }
            });
        }
    }
}
