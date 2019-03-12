using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
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
        private readonly IUserCredentialRepository _userCredentialRepository;
        private readonly ICredentialSettingsRepository _credentialSettingsRepository;

        public AddUserCredentialsOperation(IUserCredentialRepository userCredentialRepository, ICredentialSettingsRepository credentialSettingsRepository)
        {
            _userCredentialRepository = userCredentialRepository;
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

            var resourceOwnerCredentials = new List<UserCredential>();
            var currentDateTime = DateTime.UtcNow;
            foreach(var addUserCredentialParameter in addUserCredentialParameterLst)
            {
                var credentialSetting = credentialSettings.First(c => c.CredentialType == addUserCredentialParameter.CredentialType);
                resourceOwnerCredentials.Add(new UserCredential
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

            await _userCredentialRepository.Add(resourceOwnerCredentials).ConfigureAwait(false);
            return true;
        }
    }
}
