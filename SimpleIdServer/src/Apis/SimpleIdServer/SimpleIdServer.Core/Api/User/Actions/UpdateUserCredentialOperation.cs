using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IUpdateUserCredentialOperation
    {
        Task<bool> Execute(UpdateUserCredentialParameter updateUserCredentialParameter);
    }

    internal sealed class UpdateUserCredentialOperation : IUpdateUserCredentialOperation
    {
        private readonly IResourceOwnerCredentialRepository _resourceOwnerCredentialRepository;
        private readonly ICredentialSettingsRepository _credentialSettingsRepository;

        public UpdateUserCredentialOperation(IResourceOwnerCredentialRepository resourceOwnerCredentialRepository, ICredentialSettingsRepository credentialSettingsRepository)
        {
            _resourceOwnerCredentialRepository = resourceOwnerCredentialRepository;
            _credentialSettingsRepository = credentialSettingsRepository;
        }

        public async Task<bool> Execute(UpdateUserCredentialParameter updateUserCredentialParameter)
        {
            if (updateUserCredentialParameter == null)
            {
                throw new ArgumentNullException(nameof(updateUserCredentialParameter));
            }

            var credential = await _resourceOwnerCredentialRepository.GetUserCredential(updateUserCredentialParameter.UserId, updateUserCredentialParameter.CredentialType).ConfigureAwait(false);
            if (credential == null)
            {
                throw new NotFoundException();
            }

            var passwordSettings = await _credentialSettingsRepository.Get(updateUserCredentialParameter.CredentialType).ConfigureAwait(false);
            credential.Value = updateUserCredentialParameter.NewValue;
            credential.ExpirationDateTime = DateTime.UtcNow.AddSeconds(passwordSettings.ExpiresIn);
            await _resourceOwnerCredentialRepository.Update(credential).ConfigureAwait(false);
            return true;
        }
    }
}
