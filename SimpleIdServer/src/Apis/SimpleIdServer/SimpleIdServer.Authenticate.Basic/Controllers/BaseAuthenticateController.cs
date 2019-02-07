using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdServer.Authenticate.Basic.Actions;
using SimpleIdServer.Authenticate.Basic.Helpers;
using SimpleIdServer.Authenticate.Basic.ViewModels;
using SimpleIdServer.Bus;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Profile;
using SimpleIdServer.Core.Api.User;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Jwt.Extensions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Protector;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Host.Controllers.Website;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.OpenId.Events;
using SimpleIdServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.Basic.Controllers
{
    public abstract class BaseAuthenticateController : BaseController
    {
        protected const string ExternalAuthenticateCookieName = "SimpleIdentityServer-{0}";        
        protected const string DefaultLanguage = "en";
        protected readonly IAuthenticateHelper _authenticateHelper;
        protected readonly IOpenidAuthenticateResourceOwnerAction _openidAuthenticateResourceOwnerAction;
        protected readonly IProfileActions _profileActions;
        protected readonly IDataProtector _dataProtector;
        protected readonly IEncoder _encoder;
        protected readonly ITranslationManager _translationManager;        
        protected readonly IOpenIdEventSource _simpleIdentityServerEventSource;        
        protected readonly IUrlHelper _urlHelper;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        protected readonly IUserActions _userActions;
        protected readonly IPayloadSerializer _payloadSerializer;
        protected readonly IConfigurationService _configurationService;
        protected readonly BasicAuthenticateOptions _basicAuthenticateOptions;

        public BaseAuthenticateController(
            IOpenidAuthenticateResourceOwnerAction openidAuthenticateResourceOwnerAction,
            IProfileActions profileActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            IOpenIdEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUserActions userActions,
            IPayloadSerializer payloadSerializer,
            IConfigurationService configurationService,
            IAuthenticateHelper authenticateHelper,
            BasicAuthenticateOptions basicAuthenticateOptions) : base(authenticationService)
        {
            _openidAuthenticateResourceOwnerAction = openidAuthenticateResourceOwnerAction;
            _profileActions = profileActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _userActions = userActions;
            _configurationService = configurationService;
            _authenticateHelper = authenticateHelper;
            _basicAuthenticateOptions = basicAuthenticateOptions;
        }

        #region Authentication process which follows OPENID

        [HttpGet]
        public async Task<ActionResult> OpenId(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _openidAuthenticateResourceOwnerAction.Execute(request.ToParameter(), authenticatedUser, code, issuerName).ConfigureAwait(false);
            var result = this.CreateRedirectionFromActionResult(actionResult, request);
            if (result != null)
            {
                LogAuthenticateUser(actionResult, request.ProcessId);
                return result;
            }

            await TranslateView(request.UiLocales).ConfigureAwait(false);
            var viewModel = new AuthorizeOpenIdViewModel
            {
                Code = code
            };

            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View(viewModel);
        }

        [HttpPost]
        public async Task ExternalLoginOpenId(string provider, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            // 1. Persist the request code into a cookie & fix the space problems
            var cookieValue = Guid.NewGuid().ToString();
            var cookieName = string.Format(ExternalAuthenticateCookieName, cookieValue);
            Response.Cookies.Append(cookieName, code,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(5)
                });

            // 2. Redirect the User agent
            var redirectUrl = _urlHelper.AbsoluteAction("LoginCallbackOpenId", "Authenticate", new { code = cookieValue });
            await _authenticationService.ChallengeAsync(HttpContext, provider, new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            }).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<ActionResult> LoginCallbackOpenId(string code, string error)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            // 1 : retrieve the request from the cookie
            var cookieName = string.Format(ExternalAuthenticateCookieName, code);
            var request = Request.Cookies[string.Format(ExternalAuthenticateCookieName, code)];
            if (request == null)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheRequestCannotBeExtractedFromTheCookie);
            }

            // 2 : remove the cookie
            Response.Cookies.Append(cookieName, string.Empty,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(-1)
                });

            // 3 : Raise an exception is there's an authentication error
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, string.Format(ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            // 4. Check if the user is authenticated
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.ExternalCookieName).ConfigureAwait(false);
            if (authenticatedUser == null ||
                !authenticatedUser.Identity.IsAuthenticated ||
                !(authenticatedUser.Identity is ClaimsIdentity))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }

            // 5. Rerieve the claims & insert the resource owner if needed.
            var claimsIdentity = authenticatedUser.Identity as ClaimsIdentity;
            var claims = authenticatedUser.Claims.ToList();
            var resourceOwner = await _profileActions.GetResourceOwner(authenticatedUser.GetSubject()).ConfigureAwait(false);
            string sub = string.Empty;
            if (resourceOwner == null)
            {
                try
                {
                    sub = await AddExternalUser(authenticatedUser).ConfigureAwait(false);
                }
                catch (IdentityServerException ex)
                {
                    return RedirectToAction("Index", "Error", new { code = ex.Code, message = ex.Message, area = "Shell" });
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Error", new { code = ErrorCodes.InternalError, message = ex.Message, area = "Shell" });
                }
            }

            if (resourceOwner != null)
            {
                claims = resourceOwner.Claims.ToList();
            }
            else
            {
                var nameIdentifier = claims.First(c => c.Type == ClaimTypes.NameIdentifier);
                claims.Remove(nameIdentifier);
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
            }

            var subject = claims.GetSubject();
            // 6. Try to authenticate the resource owner & returns the claims.
            var authorizationRequest = _dataProtector.Unprotect<AuthorizationRequest>(request);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _authenticateHelper.ProcessRedirection(authorizationRequest.ToParameter(), request, subject, claims, issuerName).ConfigureAwait(false);

            // 7. Store claims into new cookie
            if (actionResult != null)
            {
                await SetLocalCookie(claims.ToOpenidClaims(), authorizationRequest.SessionId).ConfigureAwait(false);
                await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.ExternalCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties()).ConfigureAwait(false);
                LogAuthenticateUser(actionResult, authorizationRequest.ProcessId);
                return this.CreateRedirectionFromActionResult(actionResult, authorizationRequest);
            }

            return RedirectToAction("OpenId", "Authenticate", new { code = code });
        }
        
        #endregion
                
        #region Normal authentication process
        
        [HttpPost]
        public async Task ExternalLogin(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var redirectUrl = _urlHelper.AbsoluteAction("LoginCallback", "Authenticate");
            await _authenticationService.ChallengeAsync(HttpContext, provider, new Microsoft.AspNetCore.Authentication.AuthenticationProperties() 
            {
                RedirectUri = redirectUrl
            });
        }
        
        [HttpGet]
        public async Task<ActionResult> LoginCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, string.Format(ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            // 1. Get the authenticated user.
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.ExternalCookieName);
            var resourceOwner = await _profileActions.GetResourceOwner(authenticatedUser.GetSubject());
            string sub = null;

            // 2. Automatically create the resource owner.
            if (resourceOwner == null)
            {
                try
                {
                    sub = await AddExternalUser(authenticatedUser);
                }
                catch(IdentityServerException ex)
                {
                    return RedirectToAction("Index", "Error", new { code = ex.Code, message = ex.Message, area = "Shell" });
                }
                catch(Exception ex)
                {
                    return RedirectToAction("Index", "Error", new { code = ErrorCodes.InternalError, message = ex.Message, area = "Shell" });
                }
            }

            List<Claim> claims = authenticatedUser.Claims.ToList();
            if (resourceOwner != null)
            {
                claims = resourceOwner.Claims.ToList();
            }
            else
            {
                var nameIdentifier = claims.First(c => c.Type == ClaimTypes.NameIdentifier);
                claims.Remove(nameIdentifier);
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
            }

            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.ExternalCookieName, new AuthenticationProperties()).ConfigureAwait(false);
            // 4. Set cookie
            await SetLocalCookie(claims.ToOpenidClaims(), Guid.NewGuid().ToString()).ConfigureAwait(false);
            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.ExternalCookieName, new AuthenticationProperties()).ConfigureAwait(false);

            // 5. Redirect to the profile
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }
        
        #endregion

        #region Protected methods

        protected async Task TranslateView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Constants.StandardTranslationCodes.LoginCode,
                Constants.StandardTranslationCodes.UserNameCode,
                Constants.StandardTranslationCodes.PasswordCode,
                Constants.StandardTranslationCodes.RememberMyLoginCode,
                Constants.StandardTranslationCodes.LoginLocalAccount,
                Constants.StandardTranslationCodes.LoginExternalAccount,
                Constants.StandardTranslationCodes.SendCode,
                Constants.StandardTranslationCodes.Code,
                Constants.StandardTranslationCodes.ConfirmCode,
                Constants.StandardTranslationCodes.SendConfirmationCode,
                Constants.StandardTranslationCodes.UpdateClaim,
                Constants.StandardTranslationCodes.ConfirmationCode,
                Constants.StandardTranslationCodes.ResetConfirmationCode,
                Constants.StandardTranslationCodes.ValidateConfirmationCode,
                Constants.StandardTranslationCodes.Phone,
                Constants.StandardTranslationCodes.Email,
                Constants.StandardTranslationCodes.ActualPassword,
                Constants.StandardTranslationCodes.ConfirmActualPassword,
                Constants.StandardTranslationCodes.NewPassword,
                Constants.StandardTranslationCodes.ConfirmNewPassword,
                Constants.StandardTranslationCodes.Update,
                Constants.StandardTranslationCodes.RenewPassword
            }).ConfigureAwait(false);

            ViewBag.Translations = translations;
        }

        protected async Task SetIdProviders(AuthorizeViewModel authorizeViewModel)
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var idProviders = new List<IdProviderViewModel>();
            foreach (var scheme in schemes)
            {
                idProviders.Add(new IdProviderViewModel
                {
                    AuthenticationScheme = scheme.Name,
                    DisplayName = scheme.DisplayName
                });
            }

            authorizeViewModel.IdProviders = idProviders;
        }

        protected async Task SetIdProviders(AuthorizeOpenIdViewModel authorizeViewModel)
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var idProviders = new List<IdProviderViewModel>();
            foreach (var scheme in schemes)
            {
                idProviders.Add(new IdProviderViewModel
                {
                    AuthenticationScheme = scheme.Name,
                    DisplayName = scheme.DisplayName
                });
            }

            authorizeViewModel.IdProviders = idProviders;
        }

        /// <summary>
        /// Add an external account.
        /// </summary>
        /// <param name="authenticatedUser"></param>
        /// <returns></returns>
        protected async Task<string> AddExternalUser(ClaimsPrincipal authenticatedUser)
        {
            var openidClaims = authenticatedUser.Claims.ToOpenidClaims().ToList();
            if (_basicAuthenticateOptions.ClaimsIncludedInUserCreation != null && _basicAuthenticateOptions.ClaimsIncludedInUserCreation.Any())
            {
                var lstIndexesToRemove = openidClaims.Where(oc => !_basicAuthenticateOptions.ClaimsIncludedInUserCreation.Contains(oc.Type))
                    .Select(oc => openidClaims.IndexOf(oc))
                    .OrderByDescending(oc => oc);
                foreach (var index in lstIndexesToRemove)
                {
                    openidClaims.RemoveAt(index);
                }
            }
            
            var record = new AddUserParameter(openidClaims)
            {
                ExternalLogin = authenticatedUser.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
            };
            return await _userActions.AddUser(record, authenticatedUser.Identity.AuthenticationType).ConfigureAwait(false);
        }

        protected void LogAuthenticateUser(Core.Results.ActionResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ResourceOwnerAuthenticated(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(act), 2));
        }

        protected async Task SetLocalCookie(IEnumerable<Claim> claims, string sessionId)
        {
            var cls = claims.ToList();
            var tokenValidity = await _configurationService.GetTokenValidityPeriodInSecondsAsync().ConfigureAwait(false);
            var now = DateTime.UtcNow;
            var expires = now.AddSeconds(tokenValidity);
            HttpContext.Response.Cookies.Append(Constants.SESSION_ID, sessionId, new CookieOptions
            {
                HttpOnly = false,
                Expires = expires,
                SameSite = SameSiteMode.None
            });
            var identity = new ClaimsIdentity(cls, Host.Constants.CookieNames.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, Host.Constants.CookieNames.CookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IssuedUtc = now,
                ExpiresUtc = expires,
                AllowRefresh = false,
                IsPersistent = false
            }).ConfigureAwait(false);
        }

        protected async Task SetAcrCookie(IEnumerable<Claim> claims)
        {
            var cls = claims.ToList();
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(30);
            var identity = new ClaimsIdentity(cls, Host.Constants.CookieNames.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, Host.Constants.CookieNames.AcrCookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IssuedUtc = now,
                ExpiresUtc = expires
            }).ConfigureAwait(false);
        }

        #endregion
    }
}