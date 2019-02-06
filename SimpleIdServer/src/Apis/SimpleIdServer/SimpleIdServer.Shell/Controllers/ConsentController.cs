using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Bus;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Core.WebSite.Consent;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Host.Controllers.Website;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.OpenId.Events;
using SimpleIdServer.Shell.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants = SimpleIdServer.Host.Constants;

namespace SimpleIdServer.Shell.Controllers
{
    [Area("Shell")]
    [Authorize("Connected")]
    public class ConsentController : BaseController
    {
        private readonly IConsentActions _consentActions;
        private readonly IDataProtector _dataProtector;
        private readonly ITranslationManager _translationManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPayloadSerializer _payloadSerializer;

        public ConsentController(
            IConsentActions consentActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IPayloadSerializer payloadSerializer) : base(authenticationService)
        {
            _consentActions = consentActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _translationManager = translationManager;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
        }
        
        public async Task<ActionResult> Index(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = new Core.Common.Models.Client();
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _consentActions.DisplayConsent(request.ToParameter(), authenticatedUser.GetSubject(), issuerName).ConfigureAwait(false);
            var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
            if (result != null)
            {
                return result;
            }

            await TranslateConsentScreen(request.UiLocales).ConfigureAwait(false);
            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.ClientName,
                AllowedScopeDescriptions = actionResult.Scopes == null ? new List<string>() : actionResult.Scopes.Select(s => s.Description).ToList(),
                AllowedIndividualClaims = actionResult.AllowedClaims == null ? new List<string>() : actionResult.AllowedClaims,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                TosUri = client.TosUri,
                Code = code
            };
            return View(viewModel);
        }
        
        public async Task<ActionResult> Confirm(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var parameter = request.ToParameter();
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieNames.CookieName).ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _consentActions.ConfirmConsent(parameter, authenticatedUser.GetSubject(), issuerName).ConfigureAwait(false);
            LogConsentAccepted(actionResult, parameter.ProcessId);
            return this.CreateRedirectionFromActionResult(actionResult, request);
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted & signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public async Task<ActionResult> Cancel(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            LogConsentRejected(request.ProcessId);
            return Redirect(request.RedirectUri);
        }

        private async Task TranslateConsentScreen(string uiLocales)
        {
            // Retrieve the translation and store them in a ViewBag
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                Core.Constants.StandardTranslationCodes.ScopesCode,
                Core.Constants.StandardTranslationCodes.CancelCode,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                Core.Constants.StandardTranslationCodes.Tos
            });
            ViewBag.Translations = translations;
        }

        private void LogConsentAccepted(Core.Results.ActionResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ConsentAccepted(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(act), 10));
        }

        private void LogConsentRejected(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ConsentRejected(Guid.NewGuid().ToString(), processId, 10));
        }
    }
}