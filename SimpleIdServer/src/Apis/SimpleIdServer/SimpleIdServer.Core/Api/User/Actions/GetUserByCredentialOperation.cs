using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserByCredentialOperation
    {
        Task<IdentityStore.Models.User> Execute(string credentialType, string value);
    }

    internal sealed class GetUserByCredentialOperation : IGetUserByCredentialOperation
    {
        private readonly IUserCredentialRepository _userCredentialRepository;
        private readonly IUserRepository _userRepository;

        public GetUserByCredentialOperation(IUserCredentialRepository userCredentialRepository, IUserRepository userRepository)
        {
            _userCredentialRepository = userCredentialRepository;
            _userRepository = userRepository;
        }

        public async Task<IdentityStore.Models.User> Execute(string credentialType, string value)
        {
            if (string.IsNullOrWhiteSpace(credentialType))
            {
                throw new ArgumentNullException(nameof(credentialType));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var userCredential = await _userCredentialRepository.Get(credentialType, value).ConfigureAwait(false);
            if (userCredential == null)
            {
                return null;
            }

            return await _userRepository.Get(userCredential.UserId).ConfigureAwait(false);
        }
    }
}