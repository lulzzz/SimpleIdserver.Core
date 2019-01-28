using Newtonsoft.Json;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Actions
{
    public interface IChangePasswordAction
    {
        Task<bool> Execute(ChangePasswordParameter changePasswordParameter);
    }

    internal sealed class ChangePasswordAction : IChangePasswordAction
    {
        private readonly ICredentialSettingsRepository _passwordSettingsRepository;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public ChangePasswordAction(ICredentialSettingsRepository passwordSettingsRepository, IResourceOwnerRepository resourceOwnerRepository)
        {
            _passwordSettingsRepository = passwordSettingsRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(ChangePasswordParameter changePasswordParameter)
        {
            if (changePasswordParameter == null)
            {
                throw new ArgumentNullException(nameof(changePasswordParameter));
            }

            var resourceOwner = await _resourceOwnerRepository.GetAsync(changePasswordParameter.Subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            var credential = resourceOwner.Credentials.First(c => c.Type == Constants.AMR);
            if (credential.Value != PasswordHelper.ComputeHash(changePasswordParameter.ActualPassword))
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.ThePasswordIsNotCorrect);
            }

            var passwordSettings = await _passwordSettingsRepository.Get(Constants.AMR).ConfigureAwait(false);
            var opts = JsonConvert.DeserializeObject<PwdCredentialOptions>(passwordSettings.Options);
            if (opts.IsRegexEnabled)
            {
                var regex = new Regex(opts.RegularExpression, RegexOptions.Compiled);
                if (!regex.IsMatch(changePasswordParameter.NewPassword))
                {
                    throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, string.Format(Core.Errors.ErrorDescriptions.ThePasswordMustRespects, opts.PasswordDescription));
                }
            }

            credential.Value = PasswordHelper.ComputeHash(changePasswordParameter.NewPassword);
            credential.ExpirationDateTime = DateTime.UtcNow.AddSeconds(passwordSettings.ExpiresIn);
            await _resourceOwnerRepository.UpdateCredential(changePasswordParameter.Subject, credential).ConfigureAwait(false);
            return true;
        }
    }
}
