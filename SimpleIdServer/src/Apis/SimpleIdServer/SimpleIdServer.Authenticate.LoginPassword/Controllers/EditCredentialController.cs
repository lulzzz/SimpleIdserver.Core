using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Host.Controllers.Website;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    [Authorize("Connected")]
    public class EditCredentialController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly ITranslationManager _translationManager;
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        public EditCredentialController(IAuthenticationService authenticationService, ITranslationManager translationManager) : base(authenticationService)
        {
            _translationManager = translationManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser();
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            return View();
            // return await GetEditView(authenticatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCredentials()
        {
            return null;
        }

        private async Task TranslateUserEditView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.EditResourceOwner,
                Core.Constants.StandardTranslationCodes.NameCode,
                Core.Constants.StandardTranslationCodes.YourName,
                Core.Constants.StandardTranslationCodes.PasswordCode,
                Core.Constants.StandardTranslationCodes.YourPassword,
                Core.Constants.StandardTranslationCodes.Email,
                Core.Constants.StandardTranslationCodes.YourEmail,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Core.Constants.StandardTranslationCodes.UserIsUpdated,
                Core.Constants.StandardTranslationCodes.Phone,
                Core.Constants.StandardTranslationCodes.HashedPassword,
                Core.Constants.StandardTranslationCodes.CreateResourceOwner,
                Core.Constants.StandardTranslationCodes.Credentials,
                Core.Constants.StandardTranslationCodes.RepeatPassword,
                Core.Constants.StandardTranslationCodes.Claims,
                Core.Constants.StandardTranslationCodes.UserIsCreated,
                Core.Constants.StandardTranslationCodes.TwoFactor,
                Core.Constants.StandardTranslationCodes.NoTwoFactorAuthenticator,
                Core.Constants.StandardTranslationCodes.NoTwoFactorAuthenticatorSelected,
                Core.Constants.StandardTranslationCodes.ActualPassword,
                Core.Constants.StandardTranslationCodes.ConfirmActualPassword,
                Core.Constants.StandardTranslationCodes.NewPassword,
                Core.Constants.StandardTranslationCodes.ConfirmNewPassword
            });

            ViewBag.Translations = translations;
        }
    }
}
