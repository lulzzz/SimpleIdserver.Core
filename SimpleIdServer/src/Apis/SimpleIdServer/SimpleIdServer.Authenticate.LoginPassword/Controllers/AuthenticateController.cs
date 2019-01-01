using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Authenticate.Basic.Controllers;
using SimpleIdServer.Authenticate.Basic.ViewModels;
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
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

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
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            BasicAuthenticateOptions basicAuthenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, twoFactorAuthenticationHandler, basicAuthenticateOptions)
        {
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LocalLogin(LocalAuthenticationViewModel authorizeViewModel)
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser != null && authenticatedUser.Identity != null && authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View("Index", viewModel);
            }

            try
            {
                var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(authorizeViewModel.Login, authorizeViewModel.Password, new[] { Constants.AMR }).ConfigureAwait(false);                if (resourceOwner == null)
                {
                    throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
                }

                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
                {
                    await SetLocalCookie(claims, Guid.NewGuid().ToString()).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User", new { area = "UserManagement" });
                }

                // 2.1 Store temporary information in cookie
                await SetTwoFactorCookie(claims).ConfigureAwait(false);
                // 2.2. Send confirmation code
                try
                {
                    var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode");
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode");
                }
                catch(Exception)
                {
                    throw new Exception("Two factor authenticator is not properly configured");
                }
            }
            catch(IdentityServerUserAccountDoesntExistException)
            {
                return await DisplayError("the account doesn't exist").ConfigureAwait(false);
            }
            catch(IdentityServerUserAccountBlockedException)
            {
                return await DisplayError("the account is blocked").ConfigureAwait(false);
            }
            catch(IdentityServerUserPasswordInvalidException)
            {
                return await DisplayError("the login / password is invalid").ConfigureAwait(false);
            }
            catch (IdentityServerPasswordExpiredException ex)
            {
                await SetChangePasswordCookie(ex.ResourceOwner.Claims).ConfigureAwait(false);
                return RedirectToAction("ChangePassword", "Authenticate", new { area = Constants.AMR });
            }
            catch (IdentityServerUserTooManyRetryException ex)
            {
                return await DisplayError($"please try to connect in {ex.RetryInSeconds} seconds").ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                return await DisplayError(exception.Message).ConfigureAwait(false);
            }
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
                    await TranslateView(uiLocales);
                    await SetIdProviders(viewModel);
                    return View("OpenId", viewModel);
                }

                // 4. Local authentication
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(new LocalAuthenticationParameter
                    {
                        UserName = viewModel.Login,
                        Password = viewModel.Password
                    },
                    request.ToParameter(),
                    viewModel.Code, issuerName).ConfigureAwait(false);
                var subject = actionResult.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                // 5. Two factor authentication.
                if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
                {
					try
					{
						await SetTwoFactorCookie(actionResult.Claims);
						var code = await _authenticateActions.GenerateAndSendCode(subject);
						_simpleIdentityServerEventSource.GetConfirmationCode(code);
						return RedirectToAction("SendCode", new { code = viewModel.Code });
					}
					catch(ClaimRequiredException)
					{
						return RedirectToAction("SendCode", new { code = viewModel.Code });
					}
                    catch(Exception)
                    {
                        ModelState.AddModelError("invalid_credentials", "Two factor authenticator is not properly configured");
                    }
                }
                else
                {
                    // 6. Authenticate the user by adding a cookie
                    await SetLocalCookie(actionResult.Claims, request.SessionId).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                    // 7. Redirect the user agent
                    var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
                    if (result != null)
                    {
                        LogAuthenticateUser(actionResult.ActionResult, request.ProcessId);
                        return result;
                    }
                }
            }
            catch (IdentityServerUserAccountDoesntExistException)
            {
                _simpleIdentityServerEventSource.Failure("the account doesn't exist");
                ModelState.AddModelError("invalid_credentials", "the account doesn't exist");
            }
            catch (IdentityServerUserAccountBlockedException)
            {
                _simpleIdentityServerEventSource.Failure("the account doesn't exist");
                ModelState.AddModelError("invalid_credentials", "the account doesn't exist");
            }
            catch (IdentityServerUserPasswordInvalidException)
            {
                _simpleIdentityServerEventSource.Failure("the login / password is invalid");
                ModelState.AddModelError("invalid_credentials", "the login / password is invalid");
            }
            catch (IdentityServerPasswordExpiredException ex)
            {
                await SetChangePasswordCookie(ex.ResourceOwner.Claims).ConfigureAwait(false);
                return RedirectToAction("ChangePassword", "Authenticate", new { area = Constants.AMR, code = viewModel.Code });
            }
            catch (IdentityServerUserTooManyRetryException ex)
            {
                _simpleIdentityServerEventSource.Failure($"please try to connect in {ex.RetryInSeconds} seconds");
                ModelState.AddModelError("invalid_credentials", $"please try to connect in {ex.RetryInSeconds} seconds");
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales);
            await SetIdProviders(viewModel);
            return View("OpenId", viewModel);
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
        public async Task<IActionResult> ChangePassword(RenewPasswordViewModel viewModel)
        {
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
                await _authenticateActions.ChangePassword(new ChangePasswordParameter
                {
                    NewPassword = viewModel.NewPassword,
                    ActualPassword = viewModel.ActualPassword,
                    Subject = authenticatedUser.GetSubject()
                }).ConfigureAwait(false);
                // 2. Remove the temporary cookie and authenticate the user.
                await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.ChangePasswordCookieName, new AuthenticationProperties()).ConfigureAwait(false);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                // 3. Redirect to the user profile if no code is passed.
                if (string.IsNullOrWhiteSpace(viewModel.Code))
                {
                    await SetLocalCookie(claims, Guid.NewGuid().ToString()).ConfigureAwait(false);
                    return RedirectToAction("Index", "User", new { area = "UserManagement" });
                }

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
