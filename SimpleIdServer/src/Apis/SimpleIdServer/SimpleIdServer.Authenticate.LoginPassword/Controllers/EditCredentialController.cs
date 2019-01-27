using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Authenticate.LoginPassword.Actions;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using SimpleIdServer.Authenticate.LoginPassword.ViewModels;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Host.Controllers.Website;
using System;
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
        private readonly IChangePasswordAction _changePasswordAction;

        public EditCredentialController(IAuthenticationService authenticationService, ITranslationManager translationManager,
            IChangePasswordAction changePasswordAction) : base(authenticationService)
        {
            _translationManager = translationManager;
            _changePasswordAction = changePasswordAction;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser();
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            return View(new UpdateResourceOwnerCredentialsViewModel
            {
                Login = authenticatedUser.GetSubject()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UpdateResourceOwnerCredentialsViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            // 1. Validate the view model.
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // 2. Update the credentials
            try
            {
                var subject = authenticatedUser.GetSubject();
                await _changePasswordAction.Execute(new ChangePasswordParameter
                {
                    ActualPassword = viewModel.ActualPassword,
                    NewPassword = viewModel.NewPassword,
                    Subject = subject
                }).ConfigureAwait(false);
                ViewBag.IsUpdated = true;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error_message", ex.Message);
                return View(viewModel);
            }
        }

        private async Task TranslateUserEditView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.EditResourceOwner,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.UserIsUpdated,
                Core.Constants.StandardTranslationCodes.RepeatPassword,
                Core.Constants.StandardTranslationCodes.ActualPassword,
                Core.Constants.StandardTranslationCodes.ConfirmActualPassword,
                Core.Constants.StandardTranslationCodes.NewPassword,
                Core.Constants.StandardTranslationCodes.ConfirmNewPassword,
                Core.Constants.StandardTranslationCodes.Credentials
            });

            ViewBag.Translations = translations;
        }
    }
}
