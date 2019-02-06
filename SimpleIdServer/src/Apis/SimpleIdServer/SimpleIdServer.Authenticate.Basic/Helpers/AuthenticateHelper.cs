using SimpleIdServer.Core.Api.Authorization;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.Basic.Helpers
{
    public interface IAuthenticateHelper
    {
        Task<ActionResult> ProcessRedirection(AuthorizationParameter authorizationParameter, string code, string subject, List<Claim> claims, string issuerName);
    }

    public sealed class AuthenticateHelper : IAuthenticateHelper
    {
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IConsentHelper _consentHelper;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IClientRepository _clientRepository;
        private readonly IAmrHelper _amrHelper;

        public AuthenticateHelper(IParameterParserHelper parameterParserHelper, IActionResultFactory actionResultFactory, IConsentHelper consentHelper, IGenerateAuthorizationResponse generateAuthorizationResponse,  IClientRepository clientRepository, IAmrHelper amrHelper)
        {
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _consentHelper = consentHelper;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _clientRepository = clientRepository;
            _amrHelper = amrHelper;
        }

        public async Task<ActionResult> ProcessRedirection(AuthorizationParameter authorizationParameter, string code,  string subject, List<Claim> claims, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var client = await _clientRepository.GetClientByIdAsync(authorizationParameter.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format(ErrorDescriptions.TheClientIdDoesntExist, authorizationParameter.ClientId));
            }

            ActionResult result;
            if (authorizationParameter.AcrValues != null && authorizationParameter.AcrValues.Any())
            {
                var nextAmr = await _amrHelper.GetNextAmr(authorizationParameter.AcrValues.First(), authorizationParameter.AmrValues).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(nextAmr))
                {
                    result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                    result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                    result.AmrLst = authorizationParameter.AmrValues == null ? new List<string>() : authorizationParameter.AmrValues.ToList();
                    result.AmrLst.Add(nextAmr);
                    return result;
                }
            }

            // Redirect to the consent page if the prompt parameter contains "consent"
            var prompts = _parameterParserHelper.ParsePrompts(authorizationParameter.Prompt);
            if (prompts != null && prompts.Contains(PromptParameter.consent))
            {
                result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.AmrLst = authorizationParameter.AmrValues == null ? new List<string>() : authorizationParameter.AmrValues.ToList();
                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                result.RedirectInstruction.AddParameter("code", code);
                return result;
            }

            var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter).ConfigureAwait(false);

            // If there's already one consent then redirect to the callback
            if (assignedConsent != null)
            {
                result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                result.AmrLst = authorizationParameter.AmrValues == null ? new List<string>() : authorizationParameter.AmrValues.ToList();
                var claimsIdentity = new ClaimsIdentity(claims, "simpleIdentityServer");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter,  client, issuerName, claimsPrincipal.GetSubject()).ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
                return result;
            }

            // If there's no consent & there's no consent prompt then redirect to the consent screen.
            result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
            result.AmrLst = authorizationParameter.AmrValues == null ? new List<string>() : authorizationParameter.AmrValues.ToList();
            result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
            result.RedirectInstruction.AddParameter("code", code);
            return result;
        }

        #region Private static methods

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state)
        {
            if (responseTypes == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = SimpleIdServer.Core.Constants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Count == responseTypes.Count && k.All(key => responseTypes.Contains(key)));
            if (record == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return SimpleIdServer.Core.Constants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return SimpleIdServer.Core.Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }

        #endregion
    }
}
