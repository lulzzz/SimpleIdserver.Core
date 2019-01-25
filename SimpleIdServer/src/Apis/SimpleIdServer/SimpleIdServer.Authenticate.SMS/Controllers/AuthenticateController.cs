﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdServer.Authenticate.Basic.Controllers;
using SimpleIdServer.Authenticate.Basic.ViewModels;
using SimpleIdServer.Authenticate.SMS.Actions;
using SimpleIdServer.Authenticate.SMS.ViewModels;
using SimpleIdServer.Bus;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Profile;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
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

namespace SimpleIdServer.Authenticate.SMS.Controllers
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
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
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ISmsAuthenticationOperation smsAuthenticationOperation,
            IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            SmsAuthenticationOptions basicAuthenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, twoFactorAuthenticationHandler, basicAuthenticateOptions)
        {
            _smsAuthenticationOperation = smsAuthenticationOperation;
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser == null ||
                authenticatedUser.Identity == null ||
                !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LocalLogin(LocalAuthenticationViewModel localAuthenticationViewModel)
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            if (localAuthenticationViewModel == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationViewModel));
            }

            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(localAuthenticationViewModel.PhoneNumber).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode");
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmCode(string code)
        {
            var user = await SetUser().ConfigureAwait(false);
            if (user != null && user.Identity != null &&  user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.PasswordLessCookieName).ConfigureAwait(false);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, "SMS authentication cannot be performed");
            }

            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View(new ConfirmCodeViewModel
            {
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCode(ConfirmCodeViewModel confirmCodeViewModel)
        {
            if (confirmCodeViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmCodeViewModel));
            }

            var user = await SetUser().ConfigureAwait(false);
            if (user != null && user.Identity != null &&  user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.CookieNames.PasswordLessCookieName).ConfigureAwait(false);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, "SMS authentication cannot be performed");
            }

            var subject = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
            var phoneNumber = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
            if (confirmCodeViewModel.Action == "resend") // Resend the confirmation code.
            {
                var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value).ConfigureAwait(false);
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            ResourceOwner resourceOwner;
            try
            {
                resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(phoneNumber.Value, confirmCodeViewModel.ConfirmationCode, new[] { Constants.AMR }).ConfigureAwait(false);
            }
            catch(Exception)
            {
                ModelState.AddModelError("message_error", "Confirmation code is not valid");
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.PasswordLessCookieName, new AuthenticationProperties()).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication)) // Execute TWO Factor authentication
            {
                try
                {
                    await SetTwoFactorCookie(authenticatedUser.Claims).ConfigureAwait(false);
					var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
					_simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch(Exception)
                {
                    ModelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                    await TranslateView(DefaultLanguage).ConfigureAwait(false);
                    return View("ConfirmCode", confirmCodeViewModel);
                }
            }

            _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
            if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)) // Execute OPENID workflow
            {
                var request = _dataProtector.Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
                await SetLocalCookie(authenticatedUser.Claims, request.SessionId).ConfigureAwait(false);
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateHelper.ProcessRedirection(request.ToParameter(), confirmCodeViewModel.Code, subject, authenticatedUser.Claims.ToList(), issuerName).ConfigureAwait(false);
                var result = this.CreateRedirectionFromActionResult(actionResult, request);
                if (result != null)
                {
                    LogAuthenticateUser(actionResult, request.ProcessId);
                    return result;
                }
            }

            await SetLocalCookie(authenticatedUser.Claims, Guid.NewGuid().ToString()).ConfigureAwait(false); // Authenticate the resource owner
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
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
            // 1. Decrypt the request
            var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);
            // 2. Retrieve the default language
            uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;
            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(viewModel.PhoneNumber).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode", new { code = viewModel.Code });
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            await TranslateView(uiLocales).ConfigureAwait(false);
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("OpenId", viewModel);
        }

        private async Task SetPasswordLessCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Host.Constants.CookieNames.PasswordLessCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, Host.Constants.CookieNames.PasswordLessCookieName, principal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                IsPersistent = false
            }).ConfigureAwait(false);
        }
    }
}