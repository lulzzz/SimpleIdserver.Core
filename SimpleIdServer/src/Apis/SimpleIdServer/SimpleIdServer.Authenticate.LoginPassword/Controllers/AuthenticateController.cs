using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdServer.Authenticate.Basic.Controllers;
using SimpleIdServer.Authenticate.Basic.ViewModels;
using SimpleIdServer.Authenticate.LoginPassword.Actions;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using SimpleIdServer.Authenticate.LoginPassword.ViewModels;
using SimpleIdServer.Bus;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Profile;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Protector;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Core.WebSite.Authenticate;
using SimpleIdServer.Core.WebSite.Authenticate.Common;
using SimpleIdServer.Core.WebSite.User;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;
        private readonly IChangePasswordAction _changePasswordAction;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
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
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            IChangePasswordAction changePasswordAction,
            LoginPasswordOptions basicAuthenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, basicAuthenticateOptions)
        {
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
            _changePasswordAction = changePasswordAction;
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string code)
        {
            await SetUser().ConfigureAwait(false);
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.ChangePasswordCookieName).ConfigureAwait(false);
            if (authenticatedUser == null)
            {
                return new UnauthorizedResult();
            }

            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View(new RenewPasswordViewModel
            {
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LocalLoginOpenId(OpenidLocalAuthenticationViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                throw new ArgumentNullException(nameof(viewModel.Code));
            }

            await SetUser().ConfigureAwait(false);
            var acrUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.AcrCookieName).ConfigureAwait(false);
            var acrUserSubject = (acrUser == null || acrUser.IsAuthenticated() == false) ? null : acrUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);

                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;

                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    await TranslateView(uiLocales).ConfigureAwait(false);
                    await SetIdProviders(viewModel).ConfigureAwait(false);
                    return View("OpenId", viewModel);
                }

                // 4. Local authentication
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var parameter = request.ToParameter();
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(new LocalAuthenticationParameter { Password = viewModel.Password, UserName = viewModel.Login }, parameter, viewModel.Code, issuerName).ConfigureAwait(false);
                if (actionResult.ActionResult.AmrLst != null)
                {
                    request.AmrValues = string.Join(" ", actionResult.ActionResult.AmrLst);
                }

                if (actionResult.ActionResult.Type == Core.Results.TypeActionResult.RedirectToAction && actionResult.ActionResult.RedirectInstruction.Action == Core.Results.IdentityServerEndPoints.AuthenticateIndex)
                {
                    var encryptedRequest = _dataProtector.Protect(request);
                    actionResult.ActionResult.RedirectInstruction.AddParameter(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName, encryptedRequest);
                    await SetAcrCookie(actionResult.Claims).ConfigureAwait(false);
                    return this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
                }

                var subject = actionResult.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                // 5. Authenticate the user by adding a cookie
                await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.AcrCookieName, new AuthenticationProperties()).ConfigureAwait(false);
                await SetLocalCookie(actionResult.Claims, request.SessionId).ConfigureAwait(false);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                // 6. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
                 LogAuthenticateUser(actionResult.ActionResult, request.ProcessId);
                 return result;
            }
            catch (IdentityServerUserAccountDoesntExistException)
            {
                _simpleIdentityServerEventSource.Failure("the account doesn't exist");
                ModelState.AddModelError("invalid_credentials", "the account doesn't exist");
            }
            catch (IdentityServerUserAccountBlockedException)
            {
                _simpleIdentityServerEventSource.Failure("the account is blocked");
                ModelState.AddModelError("invalid_credentials", "the account is blocked");
            }
            catch (IdentityServerCredentialBlockedException)
            {
                _simpleIdentityServerEventSource.Failure("the credential is blocked");
                ModelState.AddModelError("invalid_credentials", "the credential is blocked");
            }
            catch (IdentityServerUserPasswordInvalidException)
            {
                _simpleIdentityServerEventSource.Failure("the login / password is invalid");
                ModelState.AddModelError("invalid_credentials", "the login / password is invalid");
            }
            catch (IdentityServerUserTooManyRetryException ex)
            {
                _simpleIdentityServerEventSource.Failure($"please try to connect in {ex.RetryInSeconds} seconds");
                ModelState.AddModelError("invalid_credentials", $"please try to connect in {ex.RetryInSeconds} seconds");
            }
            catch (IdentityServerPasswordExpiredException ex)
            {
                await SetChangePasswordCookie(ex.ResourceOwner.Claims).ConfigureAwait(false);
                return RedirectToAction("ChangePassword", "Authenticate", new { area = Constants.AMR, code = viewModel.Code });
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales).ConfigureAwait(false);
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("OpenId", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(RenewPasswordViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                throw new ArgumentNullException(nameof(viewModel.Code));
            }

            await SetUser().ConfigureAwait(false);
            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.ChangePasswordCookieName).ConfigureAwait(false);
            if (authenticatedUser == null)
            {
                return new UnauthorizedResult();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var claims = authenticatedUser.Claims.ToList();
                var subject = authenticatedUser.GetSubject();
                // 1. Change the password.
                await _changePasswordAction.Execute(new ChangePasswordParameter
                {
                    NewPassword = viewModel.NewPassword,
                    ActualPassword = viewModel.ActualPassword,
                    Subject = authenticatedUser.GetSubject()
                }).ConfigureAwait(false);
                // 2. Remove the temporary cookie and authenticate the user.
                await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.ChangePasswordCookieName, new AuthenticationProperties()).ConfigureAwait(false);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                // 4. Continue the OPENID redirection process.
                var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);
                await SetLocalCookie(claims, request.SessionId).ConfigureAwait(false);
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateHelper.ProcessRedirection(request.ToParameter(), viewModel.Code, authenticatedUser.GetSubject(), authenticatedUser.Claims.ToList(),  issuerName).ConfigureAwait(false);
                return this.CreateRedirectionFromActionResult(actionResult, request);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("error_message", ex.Message);
                return View(viewModel);
            }
        }

        private async Task<IActionResult> DisplayError(string errorMessage)
        {
            _simpleIdentityServerEventSource.Failure(errorMessage);
            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            ModelState.AddModelError("invalid_credentials", errorMessage);
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel);
            return View("Index", viewModel);
        }

        private Task SetChangePasswordCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Host.Constants.CookieNames.ChangePasswordCookieName);
            var principal = new ClaimsPrincipal(identity);
            return _authenticationService.SignInAsync(HttpContext, Host.Constants.CookieNames.ChangePasswordCookieName, principal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60),
                IsPersistent = false
            });
        }
    }
}
