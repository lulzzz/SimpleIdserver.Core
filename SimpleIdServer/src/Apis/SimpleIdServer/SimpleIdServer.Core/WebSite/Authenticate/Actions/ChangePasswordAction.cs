using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.WebSite.Authenticate.Actions
{
    public interface IChangePasswordAction
    {
        Task<bool> Execute(ChangePasswordParameter changePasswordParameter);
    }

    internal sealed class ChangePasswordAction : IChangePasswordAction
    {
        private readonly IPasswordSettingsRepository _passwordSettingsRepository;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public ChangePasswordAction(IPasswordSettingsRepository passwordSettingsRepository, IResourceOwnerRepository resourceOwnerRepository)
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
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }

            var passwordSettings = await _passwordSettingsRepository.Get().ConfigureAwait(false);
            if (passwordSettings.IsRegexEnabled)
            {
                var regex = new Regex(passwordSettings.RegularExpression, RegexOptions.Compiled);
                if (!regex.IsMatch(changePasswordParameter.NewPassword))
                {
                    throw new IdentityServerException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.ThePasswordMustRespects, passwordSettings.PasswordDescription));
                }
            }

            resourceOwner.Password = PasswordHelper.ComputeHash(changePasswordParameter.NewPassword);
            await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            return true;
        }
    }
}