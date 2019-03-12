using Newtonsoft.Json;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using SimpleIdServer.Core.Api.CredentialSettings;
using SimpleIdServer.Core.Api.User;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.IdentityStore;
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
        private readonly ICredentialSettingActions _credentialSettingActions;
        private readonly IUserActions _userActions;

        public ChangePasswordAction(ICredentialSettingActions credentialSettingActions, IUserActions userActions)
        {
            _credentialSettingActions = credentialSettingActions;
            _userActions = userActions;
        }

        public async Task<bool> Execute(ChangePasswordParameter changePasswordParameter)
        {
            if (changePasswordParameter == null)
            {
                throw new ArgumentNullException(nameof(changePasswordParameter));
            }

            var resourceOwner = await _userActions.GetUser(changePasswordParameter.Subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            var passwordSettings = await _credentialSettingActions.Get(Constants.AMR).ConfigureAwait(false);
            var opts = JsonConvert.DeserializeObject<PwdCredentialOptions>(passwordSettings.Options);
            if (opts.IsRegexEnabled)
            {
                var regex = new Regex(opts.RegularExpression, RegexOptions.Compiled);
                if (!regex.IsMatch(changePasswordParameter.NewPassword))
                {
                    throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, string.Format(Core.Errors.ErrorDescriptions.ThePasswordMustRespects, opts.PasswordDescription));
                }
            }

            var credential = resourceOwner.Credentials.FirstOrDefault(c => c.Type == Constants.AMR);
            if (credential == null)
            {
                await _userActions.AddCredentials(new[]
                {
                    new AddUserCredentialParameter
                    {
                        CredentialType = Constants.AMR,
                        UserId = changePasswordParameter.Subject,
                        Value = PasswordHelper.ComputeHash(changePasswordParameter.NewPassword)
                    }
                });
                return true;
            }

            if (!string.IsNullOrWhiteSpace(credential.Value) && credential.Value != PasswordHelper.ComputeHash(changePasswordParameter.ActualPassword))
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.ThePasswordIsNotCorrect);
            }

            await _userActions.UpdateCredential(new UpdateUserCredentialParameter
            {
                CredentialType = Constants.AMR,
                NewValue = PasswordHelper.ComputeHash(changePasswordParameter.NewPassword),
                UserId = resourceOwner.Id
            }).ConfigureAwait(false);
            return true;
        }
    }
}
