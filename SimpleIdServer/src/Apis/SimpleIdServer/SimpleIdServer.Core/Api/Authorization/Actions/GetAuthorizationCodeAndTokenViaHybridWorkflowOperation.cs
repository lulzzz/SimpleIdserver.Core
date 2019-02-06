using SimpleIdServer.Core.Api.Authorization.Common;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedSubject = null, double? authInstant = null);
    }

    public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperation : IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly IClientValidator _clientValidator;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
            IOAuthEventSource oauthEventSource,
            IProcessAuthorizationRequest processAuthorizationRequest,
            IClientValidator clientValidator,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _oauthEventSource = oauthEventSource;
            _processAuthorizationRequest = processAuthorizationRequest;
            _clientValidator = clientValidator;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedSubject = null, double? authInstant = null)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            _oauthEventSource.StartHybridFlow(
                authorizationParameter.ClientId,
                authorizationParameter.Scope,
                authorizationParameter.Claims == null ? string.Empty : authorizationParameter.Claims.ToString());
            var result = await _processAuthorizationRequest.ProcessAsync(authorizationParameter, client, issuerName, authenticatedSubject, authInstant).ConfigureAwait(false);
            if (!_clientValidator.CheckGrantTypes(client, GrantType.@implicit, GrantType.authorization_code))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"),
                    authorizationParameter.State);
            }

            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                if (string.IsNullOrWhiteSpace(authenticatedSubject))
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, client, issuerName, authenticatedSubject).ConfigureAwait(false);
            }

            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            _oauthEventSource.EndHybridFlow(
                authorizationParameter.ClientId,
                actionTypeName,
                result.RedirectInstruction == null ? string.Empty : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action));

            return result;
        }
    }
}
