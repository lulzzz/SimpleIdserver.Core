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
using SimpleIdServer.OpenId.Logging;

namespace SimpleIdServer.Core.WebSite.Consent.Actions
{
    public interface IConfirmConsentAction
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName);
    }

    public class ConfirmConsentAction : IConfirmConsentAction
    {
        private readonly IConsentRepository _consentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IScopeRepository _scopeRepository;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IConsentHelper _consentHelper;
        private readonly IOpenIdEventSource _openidEventSource;

        public ConfirmConsentAction(
            IConsentRepository consentRepository,
            IClientRepository clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IConsentHelper consentHelper,
            IOpenIdEventSource openidEventSource)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _consentHelper = consentHelper;
            _openidEventSource = openidEventSource;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName)
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
                throw new InvalidOperationException(string.Format("the client id {0} doesn't exist",
                    authorizationParameter.ClientId));
            }

            Common.Models.Consent assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(authenticatedSubject, authorizationParameter).ConfigureAwait(false);
            // Insert a new consent.
            if (assignedConsent == null)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                    claimsParameter.IsAnyUserInfoClaimParameter())
                {
                    // A consent can be given to a set of claims
                    assignedConsent = new Common.Models.Consent
                    {
                        Id = Guid.NewGuid().ToString(),
                        Client = client,
                        ResourceOwner = await _resourceOwnerRepository.GetAsync(authenticatedSubject).ConfigureAwait(false),
                        Claims = claimsParameter.GetClaimNames()
                    };
                }
                else
                {
                    // A consent can be given to a set of scopes
                    assignedConsent = new Common.Models.Consent
                    {
                        Id = Guid.NewGuid().ToString(),
                        Client = client,
                        GrantedScopes = (await GetScopes(authorizationParameter.Scope)).ToList(),
                        ResourceOwner = await _resourceOwnerRepository.GetAsync(authenticatedSubject).ConfigureAwait(false),
                    };
                }

                // A consent can be given to a set of claims
                await _consentRepository.InsertAsync(assignedConsent);

                _openidEventSource.GiveConsent(authenticatedSubject, authorizationParameter.ClientId, assignedConsent.Id);
            }

            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
            await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, client, issuerName, authenticatedSubject).ConfigureAwait(false);

            // If redirect to the callback and the responde mode has not been set.
            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
            }

            return result;
        }
        
        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private async Task<ICollection<Scope>> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = _parameterParserHelper.ParseScopes(concatenateListOfScopes);
            return await _scopeRepository.SearchByNamesAsync(scopeNames);
        }

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
    }
}
