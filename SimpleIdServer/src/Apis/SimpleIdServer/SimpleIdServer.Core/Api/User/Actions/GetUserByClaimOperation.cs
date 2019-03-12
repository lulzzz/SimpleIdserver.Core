using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserByClaimOperation
    {
        Task<IdentityStore.Models.User> Execute(string claimKey, string claimValue);
    }

    internal sealed class GetUserByClaimOperation : IGetUserByClaimOperation
    {
        private readonly IUserRepository _userRepository;

        public GetUserByClaimOperation(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<IdentityStore.Models.User> Execute(string claimKey, string claimValue)
        {
            if (string.IsNullOrWhiteSpace(claimKey))
            {
                throw new ArgumentNullException(nameof(claimKey));
            }

            if (string.IsNullOrWhiteSpace(claimValue))
            {
                throw new ArgumentNullException(nameof(claimValue));
            }

            return _userRepository.GetUserByClaim(claimKey, claimValue);
        }
    }
}
