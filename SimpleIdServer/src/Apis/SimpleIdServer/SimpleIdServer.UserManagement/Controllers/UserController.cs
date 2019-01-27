using Microsoft.AspNetCore.Authentication;
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
        private readonly UserManagementOptions _userManagementOptions;

        #region Constructor

        public UserController(IUserActions userActions, IProfileActions profileActions, ITranslationManager translationManager, 
            IAuthenticationService authenticationService, IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler, 
            IEnumerable<IEditCredentialView> editCredentialViews, UserManagementOptions userManagementOptions) : base(authenticationService)
        {
            _userActions = userActions;
            _profileActions = profileActions;
            _translationManager = translationManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            _editCredentialViews = editCredentialViews;
            _userManagementOptions = userManagementOptions;
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
            ViewBag.IsUpdated = false;
            return await DisplayEditView(authenticatedUser).ConfigureAwait(false);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateTwoFactorAuthenticatorViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (!_userManagementOptions.CanUpdateTwoFactorAuthentication)
            {
                return new NotFoundResult();
            }
            
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            await _userActions.UpdateTwoFactor(authenticatedUser.GetSubject(), viewModel.SelectedTwoFactorAuthType).ConfigureAwait(false);
            ViewBag.IsUpdated = true;
            return await DisplayEditView(authenticatedUser).ConfigureAwait(false);
        }

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
        
        private async Task TranslateUserEditView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.EditCredentialsLink,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.TwoFactor,
                Core.Constants.StandardTranslationCodes.NoTwoFactorAuthenticator,
                Core.Constants.StandardTranslationCodes.NoTwoFactorAuthenticatorSelected,
                Core.Constants.StandardTranslationCodes.UserIsUpdated
            });

            ViewBag.Translations = translations;
        }

        private async Task<IActionResult> DisplayEditView(ClaimsPrincipal authenticatedUser)
        {
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
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

            var resourceOwner = await _userActions.GetUser(authenticatedUser).ConfigureAwait(false);
            if (_userManagementOptions.CanUpdateTwoFactorAuthentication)
            {
                viewModel.SelectedTwoFactorAuthType = resourceOwner.TwoFactorAuthentication;
                viewModel.TwoFactorAuthTypes = _twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList();
            }

            viewModel.CanUpdateTwoFactorAuthentication = _userManagementOptions.CanUpdateTwoFactorAuthentication;
            return View("Edit", viewModel);
        }

        #endregion
    }
}
