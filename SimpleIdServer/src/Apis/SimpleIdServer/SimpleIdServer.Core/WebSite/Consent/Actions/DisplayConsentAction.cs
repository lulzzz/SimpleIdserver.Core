using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

namespace SimpleIdServer.Core.WebSite.Consent.Actions
{
    public interface IDisplayConsentAction
    {
        Task<DisplayContentResult> Execute(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName);
    }

    public class DisplayContentResult
    {
        public Core.Common.Models.Client Client { get; set; }
        public ICollection<Scope> Scopes { get; set; }
        public ICollection<string> AllowedClaims { get; set; }
        public ActionResult ActionResult { get; set; }
    }

    public class DisplayConsentAction : IDisplayConsentAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IConsentHelper _consentHelper;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IActionResultFactory _actionResultFactory;

        public DisplayConsentAction(
            IScopeRepository scopeRepository,
            IClientRepository clientRepository,
            IConsentHelper consentHelper,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentHelper = consentHelper;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }

        /// <returns>Action result.</returns>
        public async Task<DisplayContentResult> Execute(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (string.IsNullOrWhiteSpace(authenticatedSubject))
            {
                throw new ArgumentNullException(nameof(authenticatedSubject));
            }
            
            var client = await _clientRepository.GetClientByIdAsync(authorizationParameter.ClientId);
            if (client == null)
            {
                throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ClientIsNotValid, authorizationParameter.ClientId),
                    authorizationParameter.State);
            }

            ActionResult actionResult;
            var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(authenticatedSubject, authorizationParameter).ConfigureAwait(false);
            // If there's already a consent then redirect to the callback
            if (assignedConsent != null)
            {
                actionResult = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, client, issuerName, authenticatedSubject).ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                actionResult.RedirectInstruction.ResponseMode = responseMode;
                return new DisplayContentResult
                {
                    ActionResult = actionResult
                };
            }

            ICollection<string> allowedClaims = null;
            ICollection<Scope> allowedScopes = null;
            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                claimsParameter.IsAnyUserInfoClaimParameter())
            {
                allowedClaims = claimsParameter.GetClaimNames();
            }
            else
            {
                allowedScopes = (await GetScopes(authorizationParameter.Scope))
                    .Where(s => s.IsDisplayedInConsent)
                    .ToList();
            }

            actionResult = _actionResultFactory.CreateAnEmptyActionResultWithOutput();
            return new DisplayContentResult
            {
                AllowedClaims = allowedClaims,
                Scopes = allowedScopes,
                ActionResult = actionResult,
                Client = client
            };
        }

        private async Task<IEnumerable<Scope>> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = concatenateListOfScopes.Split(' ');
            return await _scopeRepository.SearchByNamesAsync(scopeNames);
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

            var record = Constants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Count == responseTypes.Count && k.All(key => responseTypes.Contains(key)));
            if (record == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return Constants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }

        #endregion
    }
}
