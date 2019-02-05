using Newtonsoft.Json;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Authorization.Common
{
    public interface IProcessAuthorizationRequest
    {
        Task<ActionResult> ProcessAsync(AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedUserSubject = null, double? authInstant = null);
    }

    public class ProcessAuthorizationRequest : IProcessAuthorizationRequest
    {
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IClientValidator _clientValidator;
        private readonly IScopeValidator _scopeValidator;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IConsentHelper _consentHelper;
        private readonly IJwtParser _jwtParser;
        private readonly IConfigurationService _configurationService;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IAuthenticationContextclassReferenceRepository _authenticationContextclassReferenceRepository;

        public ProcessAuthorizationRequest(
            IParameterParserHelper parameterParserHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IActionResultFactory actionResultFactory,
            IConsentHelper consentHelper,
            IJwtParser jwtParser,
            IConfigurationService configurationService,
            IOAuthEventSource oauthEventSource,
            IAuthenticationContextclassReferenceRepository authenticationContextclassReferenceRepository)
        {
            _parameterParserHelper = parameterParserHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _actionResultFactory = actionResultFactory;
            _consentHelper = consentHelper;
            _jwtParser = jwtParser;
            _configurationService = configurationService;
            _oauthEventSource = oauthEventSource;
            _authenticationContextclassReferenceRepository = authenticationContextclassReferenceRepository;
        }

        public async Task<ActionResult> ProcessAsync(AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedUserSubject = null, double? authInstant = null)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var endUserIsAuthenticated = !string.IsNullOrWhiteSpace(authenticatedUserSubject);
            Consent confirmedConsent = null;
            if (endUserIsAuthenticated)
            {
                confirmedConsent = await GetResourceOwnerConsent(authenticatedUserSubject, authorizationParameter).ConfigureAwait(false);
            }

            var serializedAuthorizationParameter = JsonConvert.SerializeObject(authorizationParameter);
            _oauthEventSource.StartProcessingAuthorizationRequest(serializedAuthorizationParameter);
            ActionResult result = null;
            var prompts = _parameterParserHelper.ParsePrompts(authorizationParameter.Prompt);
            if (prompts == null || !prompts.Any())
            {
                prompts = new List<PromptParameter>();
                if (!endUserIsAuthenticated)
                {
                    prompts.Add(PromptParameter.login);
                }
                else
                {
                    if (confirmedConsent == null)
                    {
                        prompts.Add(PromptParameter.consent);
                    }
                    else
                    {
                        prompts.Add(PromptParameter.none);
                    }
                }
            }

            var redirectionUrls = _clientValidator.GetRedirectionUrls(client, authorizationParameter.RedirectUrl);
            if (!redirectionUrls.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationParameter.RedirectUrl),
                    authorizationParameter.State);
            }

            var scopeValidationResult = _scopeValidator.Check(authorizationParameter.Scope, client);
            if (!scopeValidationResult.IsValid)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidScope,
                    scopeValidationResult.ErrorMessage,
                    authorizationParameter.State);
            }

            if (!scopeValidationResult.Scopes.Contains(Constants.StandardScopes.OpenId.Name))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidScope,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, Constants.StandardScopes.OpenId.Name),
                    authorizationParameter.State);
            }

            var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            if (!responseTypes.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    authorizationParameter.State);
            }

            if (!_clientValidator.CheckResponseTypes(client, responseTypes.ToArray()))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        authorizationParameter.ClientId,
                        string.Join(",", responseTypes)),
                    authorizationParameter.State);
            }

            // Check if the user connection is still valid.
            if (endUserIsAuthenticated && !authorizationParameter.MaxAge.Equals(default(double)))
            {
                if (authInstant != null)
                {
                    var maxAge = authorizationParameter.MaxAge;
                    var currentDateTimeUtc = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
                    if (maxAge < currentDateTimeUtc - authInstant.Value)
                    {
                        result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                        result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                        await SetAcr(authorizationParameter, result).ConfigureAwait(false);
                    }
                }
            }

            if (result == null)
            {
                result = await ProcessPromptParameters(prompts, endUserIsAuthenticated, authorizationParameter, confirmedConsent).ConfigureAwait(false);
                await ProcessIdTokenHint(result, authorizationParameter, prompts, authenticatedUserSubject, issuerName).ConfigureAwait(false);
            }

            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            var actionName = result.RedirectInstruction == null 
                ? string.Empty 
                : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action);
            _oauthEventSource.EndProcessingAuthorizationRequest(
                serializedAuthorizationParameter,
                actionTypeName,
                actionName);

            return result;
        }

        private async Task ProcessIdTokenHint(ActionResult actionResult, 
            AuthorizationParameter authorizationParameter, 
            ICollection<PromptParameter> prompts, 
            string currentSubject,
            string issuerName)
        {
            if (!string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint) &&
                prompts.Contains(PromptParameter.none) &&
                actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var token = authorizationParameter.IdTokenHint;
                if (!_jwtParser.IsJweToken(token) &&
                    !_jwtParser.IsJwsToken(token))
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheIdTokenHintParameterIsNotAValidToken,
                            authorizationParameter.State);
                }

                string jwsToken;
                if (_jwtParser.IsJweToken(token))
                {
                    jwsToken = await _jwtParser.DecryptAsync(token);
                    if (string.IsNullOrWhiteSpace(jwsToken))
                    {
                        throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheIdTokenHintParameterCannotBeDecrypted,
                            authorizationParameter.State);
                    }
                }
                else
                {
                    jwsToken = token;
                }

                var jwsPayload = await _jwtParser.UnSignAsync(jwsToken);
                if (jwsPayload == null)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheSignatureOfIdTokenHintParameterCannotBeChecked,
                        authorizationParameter.State);
                }

                if (jwsPayload.Audiences == null ||
                    !jwsPayload.Audiences.Any() ||
                    !jwsPayload.Audiences.Contains(issuerName))
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience,
                        authorizationParameter.State);
                }
                
                var expectedSubject = jwsPayload.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                if (currentSubject != expectedSubject)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken,
                        authorizationParameter.State);
                }
            }
        }

        private async Task SetAcr(AuthorizationParameter authorizationParameter, ActionResult actionResult)
        {
            if (authorizationParameter.AcrValues == null || !authorizationParameter.AcrValues.Any())
            {
                await SetDefaultAcr(actionResult).ConfigureAwait(false);
                return;
            }

            var acrLst = await _authenticationContextclassReferenceRepository.Get(authorizationParameter.AcrValues).ConfigureAwait(false);
            var selectedAcrName = authorizationParameter.AcrValues.FirstOrDefault(a => acrLst.Any(ac => ac.Name == a));
            var selectedAcr = acrLst.FirstOrDefault(a => a.Name == selectedAcrName);
            if (selectedAcr == null)
            {
                await SetDefaultAcr(actionResult).ConfigureAwait(false);
                return;
            }

            actionResult.Acr = selectedAcr.Name;
            actionResult.AmrLst = new List<string> { selectedAcr.AmrLst.First() };
        }

        private async Task SetDefaultAcr(ActionResult actionResult)
        {
            var acr = await _authenticationContextclassReferenceRepository.GetDefault().ConfigureAwait(false);
            actionResult.Acr = acr.Name;
            actionResult.AmrLst = new List<string> { acr.AmrLst.First() };
        }

        private async Task<ActionResult> ProcessPromptParameters(ICollection<PromptParameter> prompts, bool isAuthenticated, AuthorizationParameter authorizationParameter, Consent confirmedConsent)
        {
            if (prompts == null || !prompts.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt,
                    authorizationParameter.State);
            }

            // Raise "login_required" exception : if the prompt authorizationParameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameter.none))
            {
                if (!isAuthenticated)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.LoginRequiredCode,
                        ErrorDescriptions.TheUserNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }
                
                if (confirmedConsent == null)
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InteractionRequiredCode,
                            ErrorDescriptions.TheUserNeedsToGiveHisConsent,
                            authorizationParameter.State);
                }

                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                return result;
            }

            // Redirects to the authentication screen 
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
            if (prompts.Contains(PromptParameter.login))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                await SetAcr(authorizationParameter, result).ConfigureAwait(false);
                return result;
            }

            if (prompts.Contains(PromptParameter.consent))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                if (!isAuthenticated)
                {
                    result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                    await SetAcr(authorizationParameter, result).ConfigureAwait(false);
                    return result;
                }

                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                return result;
            }

            throw new IdentityServerExceptionWithState(
                ErrorCodes.InvalidRequestCode,
                string.Format(ErrorDescriptions.ThePromptParameterIsNotSupported, string.Join(",", prompts)),
                authorizationParameter.State);
        }

        private async Task<Consent> GetResourceOwnerConsent(string authenticatedUserSubject, AuthorizationParameter authorizationParameter)
        {
            return await _consentHelper.GetConfirmedConsentsAsync(authenticatedUserSubject, authorizationParameter);
        }
    }
}
