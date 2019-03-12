using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Profile.Actions
{
    public interface IGetResourceOwnerClaimsAction
    {
        Task<IdentityStore.Models.User> Execute(string externalSubject);
    }

    internal sealed class GetResourceOwnerClaimsAction : IGetResourceOwnerClaimsAction
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public GetResourceOwnerClaimsAction(IProfileRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public async Task<IdentityStore.Models.User> Execute(string externalSubject)
        {
            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            var profile = await _profileRepository.Get(externalSubject);
            if (profile == null)
            {
                return null;
            }

            return await _userRepository.Get(profile.UserId);
        }
    }
}
