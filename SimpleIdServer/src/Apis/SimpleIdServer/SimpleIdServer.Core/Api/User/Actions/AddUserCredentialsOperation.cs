using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IAddUserCredentialsOperation
    {
        Task<bool> Execute(IEnumerable<AddUserCredentialParameter> addUserCredentialParameterLst);
    }

    internal sealed class AddUserCredentialsOperation : IAddUserCredentialsOperation
    {
        private readonly IResourceOwnerCredentialRepository _resourceOwnerCredentialRepository;
        private readonly ICredentialSettingsRepository _credentialSettingsRepository;

        public AddUserCredentialsOperation(IResourceOwnerCredentialRepository resourceOwnerCredentialRepository, ICredentialSettingsRepository credentialSettingsRepository)
        {
            _resourceOwnerCredentialRepository = resourceOwnerCredentialRepository;
            _credentialSettingsRepository = credentialSettingsRepository;
        }

        public async Task<bool> Execute(IEnumerable<AddUserCredentialParameter> addUserCredentialParameterLst)
        {
            if (addUserCredentialParameterLst == null)
            {
                throw new ArgumentNullException(nameof(addUserCredentialParameterLst));
            }

            var credentialSettings = await _credentialSettingsRepository.Get(addUserCredentialParameterLst.Select(a => a.CredentialType)).ConfigureAwait(false);
            if (credentialSettings.Count() != addUserCredentialParameterLst.Count())
            {
                throw new IdentityServerException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.SomeCredentialsAreNotValid);
            }

            var resourceOwnerCredentials = new List<ResourceOwnerCredential>();
            var currentDateTime = DateTime.UtcNow;
            foreach(var addUserCredentialParameter in addUserCredentialParameterLst)
            {
                var credentialSetting = credentialSettings.First(c => c.CredentialType == addUserCredentialParameter.CredentialType);
                resourceOwnerCredentials.Add(new ResourceOwnerCredential
                {
                    UserId = addUserCredentialParameter.UserId,
                    ExpirationDateTime = currentDateTime.AddSeconds(credentialSetting.ExpiresIn),
                    FirstAuthenticationFailureDateTime = null,
                    IsBlocked = false,
                    Value = addUserCredentialParameter.Value,
                    Type = addUserCredentialParameter.CredentialType,
                    NumberOfAttempts = 0
                });
            }

            await _resourceOwnerCredentialRepository.Add(resourceOwnerCredentials).ConfigureAwait(false);
            return true;
        }
    }
}
