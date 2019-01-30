using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserByClaimOperation
    {
        Task<ResourceOwner> Execute(string claimKey, string claimValue);
    }

    internal sealed class GetUserByClaimOperation : IGetUserByClaimOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetUserByClaimOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public Task<ResourceOwner> Execute(string claimKey, string claimValue)
        {
            if (string.IsNullOrWhiteSpace(claimKey))
            {
                throw new ArgumentNullException(nameof(claimKey));
            }

            if (string.IsNullOrWhiteSpace(claimValue))
            {
                throw new ArgumentNullException(nameof(claimValue));
            }

            return _resourceOwnerRepository.GetResourceOwnerByClaim(claimKey, claimValue);
        }
    }
}
