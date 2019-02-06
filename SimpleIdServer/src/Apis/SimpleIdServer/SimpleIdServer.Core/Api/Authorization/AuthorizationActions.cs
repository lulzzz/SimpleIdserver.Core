using Newtonsoft.Json;
using SimpleIdServer.Bus;
using SimpleIdServer.Core.Api.Authorization.Actions;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Events;
using SimpleIdServer.OAuth.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Authorization
{
    public interface IAuthorizationActions
    {
        Task<ActionResult> GetAuthorization(AuthorizationParameter parameter, string issuerName, string authenticatedSubject = null, double? authInstant = null);
    }

    public class AuthorizationActions : IAuthorizationActions
    {
        private readonly IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;
        private readonly IGetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;
        private readonly IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;
        private readonly IAuthorizationCodeGrantTypeParameterAuthEdpValidator _authorizationCodeGrantTypeParameterValidator;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPayloadSerializer _payloadSerializer;
        private readonly IAmrHelper _amrHelper;
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        public AuthorizationActions(
            IGetAuthorizationCodeOperation getAuthorizationCodeOperation,
            IGetTokenViaImplicitWorkflowOperation getTokenViaImplicitWorkflowOperation,
            IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation getAuthorizationCodeAndTokenViaHybridWorkflowOperation,
            IAuthorizationCodeGrantTypeParameterAuthEdpValidator authorizationCodeGrantTypeParameterValidator,
            IParameterParserHelper parameterParserHelper,
            IOAuthEventSource oauthEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IEventPublisher eventPublisher,
            IPayloadSerializer payloadSerializer, 
            IAmrHelper amrHelper,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper)
        {
            _getAuthorizationCodeOperation = getAuthorizationCodeOperation;
            _getTokenViaImplicitWorkflowOperation = getTokenViaImplicitWorkflowOperation;
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                getAuthorizationCodeAndTokenViaHybridWorkflowOperation;
            _authorizationCodeGrantTypeParameterValidator = authorizationCodeGrantTypeParameterValidator;
            _parameterParserHelper = parameterParserHelper;
            _oauthEventSource = oauthEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
            _amrHelper = amrHelper;
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

        public async Task<ActionResult> GetAuthorization(AuthorizationParameter parameter, string issuerName, string authenticatedSubject = null, double? authInstant = null)
        {
            var processId = Guid.NewGuid().ToString();
            _eventPublisher.Publish(new AuthorizationRequestReceived(Guid.NewGuid().ToString(), processId,  _payloadSerializer.GetPayload(parameter), 0));
            try
            {
                var client = await _authorizationCodeGrantTypeParameterValidator.ValidateAsync(parameter);
                ActionResult actionResult = null;
                _oauthEventSource.StartAuthorization(parameter.ClientId,
                    parameter.ResponseType,
                    parameter.Scope,
                    parameter.Claims == null ? string.Empty : parameter.Claims.ToString());
                if (client.RequirePkce)
                {
                    if (string.IsNullOrWhiteSpace(parameter.CodeChallenge) || parameter.CodeChallengeMethod == null)
                    {
                        throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheClientRequiresPkce, parameter.ClientId), parameter.State);
                    }
                }

                var responseTypes = _parameterParserHelper.ParseResponseTypes(parameter.ResponseType);
                var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes, parameter.State);
                switch (authorizationFlow)
                {
                    case AuthorizationFlow.AuthorizationCodeFlow:
                        actionResult = await _getAuthorizationCodeOperation.Execute(parameter, client, issuerName, authenticatedSubject, authInstant);
                        break;
                    case AuthorizationFlow.ImplicitFlow:
                        actionResult = await _getTokenViaImplicitWorkflowOperation.Execute(parameter, client, issuerName, authenticatedSubject, authInstant);
                        break;
                    case AuthorizationFlow.HybridFlow:
                        actionResult = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(parameter, client, issuerName, authenticatedSubject, authInstant);
                        break;
                }

                if (actionResult != null)
                {
                    var actionTypeName = Enum.GetName(typeof(TypeActionResult), actionResult.Type);
                    var actionName = string.Empty;
                    if (actionResult.Type == TypeActionResult.RedirectToAction)
                    {
                        var actionEnum = actionResult.RedirectInstruction.Action;
                        actionName = Enum.GetName(typeof(IdentityServerEndPoints), actionEnum);
                    }

                    var serializedParameters = actionResult.RedirectInstruction == null || actionResult.RedirectInstruction.Parameters == null ? String.Empty :
                        JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters);
                    _oauthEventSource.EndAuthorization(actionTypeName,
                        actionName,
                        serializedParameters);
                }

                _eventPublisher.Publish(new AuthorizationGranted(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(actionResult), 1));
                actionResult.ProcessId = processId;
                return actionResult;
            }
            catch(IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }
    }
}
