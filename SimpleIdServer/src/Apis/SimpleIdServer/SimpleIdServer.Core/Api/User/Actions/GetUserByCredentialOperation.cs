using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserByCredentialOperation
    {
        Task<ResourceOwner> Execute(string credentialType, string value);
    }

    internal sealed class GetUserByCredentialOperation : IGetUserByCredentialOperation
    {
        private readonly IResourceOwnerCredentialRepository _resourceOwnerCredentialRepository;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetUserByCredentialOperation(IResourceOwnerCredentialRepository resourceOwnerCredentialRepository, IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerCredentialRepository = resourceOwnerCredentialRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> Execute(string credentialType, string value)
        {
            if (string.IsNullOrWhiteSpace(credentialType))
            {
                throw new ArgumentNullException(nameof(credentialType));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var userCredential = await _resourceOwnerCredentialRepository.Get(credentialType, value).ConfigureAwait(false);
            if (userCredential == null)
            {
                return null;
            }

            return await _resourceOwnerRepository.GetAsync(userCredential.UserId).ConfigureAwait(false);
        }
    }
}