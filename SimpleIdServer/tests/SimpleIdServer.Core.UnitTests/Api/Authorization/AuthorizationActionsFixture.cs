﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using SimpleIdServer.Bus;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Authorization;
using SimpleIdServer.Core.Api.Authorization.Actions;
using SimpleIdServer.Lib;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;
using Xunit;
using Newtonsoft.Json;

namespace SimpleIdentityServer.Core.UnitTests.Api.Authorization
{
    public sealed class AuthorizationActionsFixture
    {
        private Mock<IGetAuthorizationCodeOperation> _getAuthorizationCodeOperationFake;
        private Mock<IGetTokenViaImplicitWorkflowOperation> _getTokenViaImplicitWorkflowOperationFake;
        private Mock<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation>
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperationFake;
        private Mock<IAuthorizationCodeGrantTypeParameterAuthEdpValidator> _authorizationCodeGrantTypeParameterAuthEdpValidatorFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IAuthorizationFlowHelper> _authorizationFlowHelperFake;
        private Mock<IEventPublisher> _eventPublisherStub;
        private Mock<IPayloadSerializer> _payloadSerializerStub;
        private Mock<IAmrHelper> _amrHelperStub;
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperStub;
        private IAuthorizationActions _authorizationActions;

        [Fact]
        public async Task When_Client_Require_PKCE_And_NoCodeChallenge_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            InitializeFakeObjects();
            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Setup(a => a.ValidateAsync(It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(new Client
                {
                    RequirePkce = true,
                    ClientId = clientId
                }));

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
            };

            // ACT & ASSERT
            var result = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _authorizationActions.GetAuthorization(authorizationParameter, null, null));
            Assert.True(result.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(result.Message == string.Format(ErrorDescriptions.TheClientRequiresPkce, clientId));
        }

        [Fact]
        public void When_Starting_Implicit_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            // ARRANGE
            InitializeFakeObjects();
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };

            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Setup(a => a.ValidateAsync(It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(new Client
                {
                    RequirePkce = false
                }));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token
                });
            _getTokenViaImplicitWorkflowOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
                It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<ResponseType>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.ImplicitFlow);

            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            const string actionType = "RedirectToAction";
            const string controllerAction = "ConsentIndex";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null
            };
            var serializedParameter = JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters);

            // ACT
            _authorizationActions.GetAuthorization(authorizationParameter, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        [Fact]
        public void When_Starting_AuthorizationCode_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            // ARRANGE
            InitializeFakeObjects();
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };

            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Setup(a => a.ValidateAsync(It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(new Client
                {
                    RequirePkce = false
                }));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token
                });
            _getAuthorizationCodeOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
                It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<ResponseType>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.AuthorizationCodeFlow);

            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            const string actionType = "RedirectToAction";
            const string controllerAction = "ConsentIndex";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null
            };
            var serializedParameter = JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters);

            // ACT
            _authorizationActions.GetAuthorization(authorizationParameter, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        [Fact]
        public void When_Starting_Hybrid_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            // ARRANGE
            InitializeFakeObjects();
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };

            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Setup(a => a.ValidateAsync(It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(new Client
                {
                    RequirePkce = false
                }));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token
                });
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
                It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<ResponseType>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.HybridFlow);

            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            const string actionType = "RedirectToAction";
            const string controllerAction = "ConsentIndex";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null
            };
            var serializedParameter = JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters);

            // ACT
            _authorizationActions.GetAuthorization(authorizationParameter, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        private void InitializeFakeObjects()
        {
            _getAuthorizationCodeOperationFake = new Mock<IGetAuthorizationCodeOperation>();
            _getTokenViaImplicitWorkflowOperationFake = new Mock<IGetTokenViaImplicitWorkflowOperation>();
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperationFake = new Mock<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();
            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake =
                new Mock<IAuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authorizationFlowHelperFake = new Mock<IAuthorizationFlowHelper>();
            _eventPublisherStub = new Mock<IEventPublisher>();
            _payloadSerializerStub = new Mock<IPayloadSerializer>();
            _amrHelperStub = new Mock<IAmrHelper>();
            _resourceOwnerAuthenticateHelperStub = new Mock<IResourceOwnerAuthenticateHelper>();
            _authorizationActions = new AuthorizationActions(
                _getAuthorizationCodeOperationFake.Object,
                _getTokenViaImplicitWorkflowOperationFake.Object,
                _getAuthorizationCodeAndTokenViaHybridWorkflowOperationFake.Object,
                _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Object,
                _parameterParserHelperFake.Object,
                _oauthEventSource.Object,
                _authorizationFlowHelperFake.Object,
                _eventPublisherStub.Object,
                _payloadSerializerStub.Object,
                _amrHelperStub.Object,
                _resourceOwnerAuthenticateHelperStub.Object);
        }
    }
}
