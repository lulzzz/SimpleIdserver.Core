using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Profile.Actions
{
    public interface IGetUserProfilesAction
    {
        Task<IEnumerable<ResourceOwnerProfile>> Execute(string subject);
    }

    internal sealed class GetUserProfilesAction : IGetUserProfilesAction
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;

        public GetUserProfilesAction(IProfileRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get the profiles linked to the user account.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ResourceOwnerProfile>> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }


            var resourceOwner = await _userRepository.Get(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            return await _profileRepository.Search(new SearchProfileParameter
            {
                ResourceOwnerIds = new[] { subject }
            });
        }
    }
}
