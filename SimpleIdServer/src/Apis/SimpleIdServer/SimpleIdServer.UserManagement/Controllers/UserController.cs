﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdServer.Core.Api.Profile;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Core.WebSite.User;
using SimpleIdServer.Host;
using SimpleIdServer.Host.Controllers.Website;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.Module;
using SimpleIdServer.UserManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize("Connected")]
    public class UserController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly IUserActions _userActions;
        private readonly IProfileActions _profileActions;
        private readonly ITranslationManager _translationManager;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IUrlHelper _urlHelper;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
        private readonly IEnumerable<IEditCredentialView> _editCredentialViews;

        #region Constructor

        public UserController(IUserActions userActions, IProfileActions profileActions, ITranslationManager translationManager, 
            IAuthenticationService authenticationService, IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler, 
            IEnumerable<IEditCredentialView> editCredentialViews) : base(authenticationService)
        {
            _userActions = userActions;
            _profileActions = profileActions;
            _translationManager = translationManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            _editCredentialViews = editCredentialViews;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            await SetUser().ConfigureAwait(false);
            return await GetConsents();
        }

        [HttpPost]
        public async Task<ActionResult> Consent(string id)
        {
            if (!await _userActions.DeleteConsent(id).ConfigureAwait(false))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return await GetConsents();
            }

            return RedirectToAction("Consent");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var viewModel = new EditCredentialViewModel();
            if (_editCredentialViews != null)
            {
                viewModel.Links = _editCredentialViews.Where(e => e.IsEnabled).Select(e => {
                    return new EditCredentialLinkViewModel
                    {
                        DisplayName = e.DisplayName,
                        Href = e.Href
                    };
                }).ToList();
            }
            return View(viewModel);
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCredentials(UpdateResourceOwnerCredentialsViewModel viewModel)
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
                return await GetEditView(authenticatedUser).ConfigureAwait(false);
            }

            // 2. Update the credentials
            try
            {
                var resourceOwner = await _userActions.GetUser(authenticatedUser).ConfigureAwait(false);
                var subject = authenticatedUser.GetSubject();
                await _userActions.UpdateCredentials(new Core.Parameters.ChangePasswordParameter
                {
                    ActualPassword = viewModel.ActualPassword,
                    NewPassword = viewModel.NewPassword,
                    Subject = subject
                }).ConfigureAwait(false);
                ViewBag.IsUpdated = true;
                return await GetEditView(authenticatedUser).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("error_message", ex.Message);
                return await GetEditView(authenticatedUser).ConfigureAwait(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTwoFactor(UpdateTwoFactorAuthenticatorViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
            
            await TranslateUserEditView(DefaultLanguage);
            var authenticatedUser = await SetUser();
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            await _userActions.UpdateTwoFactor(authenticatedUser.GetSubject(), viewModel.SelectedTwoFactorAuthType);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser);
        }
        */

        /// <summary>
        /// Display the profiles linked to the user account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authenticatedUser = await SetUser();
            var actualScheme = authenticatedUser.Identity.AuthenticationType;
            var profiles = await _profileActions.GetProfiles(authenticatedUser.GetSubject());
            var authenticationSchemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).Where(a => !string.IsNullOrWhiteSpace(a.DisplayName));
            var viewModel = new ProfileViewModel();
            if (profiles != null && profiles.Any())
            {
                foreach (var profile in profiles)
                {
                    var record = new IdentityProviderViewModel(profile.Issuer, profile.Subject);
                    viewModel.LinkedIdentityProviders.Add(record);
                }
            }
            
            viewModel.UnlinkedIdentityProviders = authenticationSchemes.Where(a => profiles != null && !profiles.Any(p => p.Issuer == a.Name && a.Name != actualScheme))
                .Select(p => new IdentityProviderViewModel(p.Name)).ToList();
            return View("Profile", viewModel);
        }

        /// <summary>
        /// Link an external account to the local one.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Link(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var redirectUrl = _urlHelper.AbsoluteAction("LinkCallback", "User", new { area = "UserManagement" });
            await _authenticationService.ChallengeAsync(HttpContext, provider, new AuthenticationProperties()
            {
                RedirectUri = redirectUrl
            });
        }
        
        /// <summary>
        /// Callback operation used to link an external account to the local one.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LinkCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, string.Format(ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            try
            {
                var authenticatedUser = await SetUser();
                var externalClaims = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieNames.ExternalCookieName);                
                var resourceOwner = await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, false);
                await _authenticationService.SignOutAsync(HttpContext, Constants.CookieNames.ExternalCookieName, new AuthenticationProperties());
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            catch (ProfileAssignedAnotherAccountException)
            {
                return RedirectToAction("LinkProfileConfirmation");
            }
            catch (Exception)
            {
                await _authenticationService.SignOutAsync(HttpContext, Constants.CookieNames.ExternalCookieName, new AuthenticationProperties());
                throw;
            }
        }

        /// <summary>
        /// Confirm to link the external account to this local account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LinkProfileConfirmation()
        {
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieNames.ExternalCookieName);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            await SetUser();
            var authenticationType = ((ClaimsIdentity)externalClaims.Identity).AuthenticationType;
            var viewModel = new LinkProfileConfirmationViewModel(authenticationType);
            return View(viewModel);
        }
        
        /// <summary>
        /// Force to link the external account to the local one.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmProfileLinking()
        {
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieNames.ExternalCookieName);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await SetUser();
            try
            {
                await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, true);
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            finally
            {
                await _authenticationService.SignOutAsync(HttpContext, Constants.CookieNames.ExternalCookieName, new AuthenticationProperties());
            }
        }
        
        /// <summary>
        /// Unlink the external account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Unlink(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            var authenticatedUser = await SetUser();
            try
            {
                await _profileActions.Unlink(authenticatedUser.GetSubject(), id);
            }
            catch (IdentityServerException ex)
            {
                return RedirectToAction("Index", "Error", new { code = ex.Code, message = ex.Message, area = "Shell" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { code = ErrorCodes.InternalError, message = ex.Message, area = "Shell" });
            }

            return await Profile();
        }

        #endregion

        #region Private methods

        /*
        private async Task<IActionResult> GetEditView(ClaimsPrincipal authenticatedUser)
        {
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            UpdateResourceOwnerViewModel viewModel = null;
            if (resourceOwner == null)
            {
                viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication, authenticatedUser.GetSubject(), authenticatedUser.Claims, false);
                return View("Edit", viewModel);
            }

            viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication, authenticatedUser.GetSubject(), resourceOwner.Claims, true);
            viewModel.IsLocalAccount = true;
            return View("Edit", viewModel);
        }
        */

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await SetUser();
            var consents = await _userActions.GetConsents(authenticatedUser);
            var result = new List<ConsentViewModel>();
            if (consents != null)
            {
                foreach (var consent in consents)
                {
                    var client = consent.Client;
                    var scopes = consent.GrantedScopes;
                    var claims = consent.Claims;
                    var viewModel = new ConsentViewModel
                    {
                        Id = consent.Id,
                        ClientDisplayName = client == null ? string.Empty : client.ClientName,
                        AllowedScopeDescriptions = scopes == null || !scopes.Any() ?
                            new List<string>() :
                            scopes.Select(g => g.Description).ToList(),
                        AllowedIndividualClaims = claims == null ? new List<string>() : claims,
                        LogoUri = client == null ? string.Empty : client.LogoUri,
                        PolicyUri = client == null ? string.Empty : client.PolicyUri,
                        TosUri = client == null ? string.Empty : client.TosUri
                    };

                    result.Add(viewModel);
                }
            }

            return View(result);
        }

        /*
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

        private UpdateResourceOwnerViewModel BuildViewModel(string twoFactorAuthType, string subject, IEnumerable<Claim> claims, bool isLocalAccount)
        {
            var editableClaims = new Dictionary<string, string>();
            var notEditableClaims = new Dictionary<string, string>();
            foreach(var claim in claims)
            {
                if (Core.Jwt.Constants.NotEditableResourceOwnerClaimNames.Contains(claim.Type))
                {
                    notEditableClaims.Add(claim.Type, claim.Value);
                }
                else
                {
                    editableClaims.Add(claim.Type, claim.Value);
                }
            }
            
            var result = new UpdateResourceOwnerViewModel(subject, editableClaims, notEditableClaims, isLocalAccount);
            result.SelectedTwoFactorAuthType = twoFactorAuthType;
            result.TwoFactorAuthTypes = _twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList();
            return result;
        }
        */

        #endregion
    }
}
