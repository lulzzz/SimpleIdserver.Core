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
    public interface IGetAuthorizationCodeOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedSubject = null, long? authInstant = null);
    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly IClientValidator _clientValidator;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IOAuthEventSource _oAuthEventSource;

        public GetAuthorizationCodeOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IClientValidator clientValidator,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IOAuthEventSource oAuthEventSource)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _clientValidator = clientValidator;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _oAuthEventSource = oAuthEventSource;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, Core.Common.Models.Client client, string issuerName, string authenticatedSubject = null, long? authInst = null)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            
            _oAuthEventSource.StartAuthorizationCodeFlow(
                authorizationParameter.ClientId,
                authorizationParameter.Scope,
                authorizationParameter.Claims == null ? string.Empty : authorizationParameter.Claims.ToString());
            var result = await _processAuthorizationRequest.ProcessAsync(authorizationParameter, client, issuerName, authenticatedSubject, authInst);
            if (!_clientValidator.CheckGrantTypes(client, GrantType.authorization_code)) // 1. Check the client is authorized to use the authorization_code flow.
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "authorization_code"),
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
            _oAuthEventSource.EndAuthorizationCodeFlow(
                authorizationParameter.ClientId,
                actionTypeName,
                result.RedirectInstruction == null ? string.Empty : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action));

            return result;
        }
    }
}
