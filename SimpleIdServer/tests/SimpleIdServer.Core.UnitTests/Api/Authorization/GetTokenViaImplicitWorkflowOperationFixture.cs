﻿using System;
using System.Security.Claims;
using Moq;
using Xunit;
using System.Threading.Tasks;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Authorization.Actions;
using SimpleIdServer.Core.Api.Authorization.Common;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;

namespace SimpleIdentityServer.Core.UnitTests.Api.Authorization
{
    public class GetTokenViaImplicitWorkflowOperationFixture
    {
        private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IClientValidator> _clientValidatorFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private IGetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(new AuthorizationParameter(), null, null, null));
        }

        [Fact]
        public async Task When_Passing_No_Nonce_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.NonceName));
            Assert.True(exception.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Implicit_Flow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = "nonce",
                State = "state"
            };

            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(false);

            // ACT & ASSERTS
            var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null));
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Requesting_Authorization_With_Valid_Request_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state",
                Nonce =  "nonce",
                ClientId =  clientId,
                Scope = scope,
                Claims = null
            };
            var actionResult = new ActionResult()
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

            // ACT
            await _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, claimsPrincipal, new Client(), null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartImplicitFlow(clientId, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndImplicitFlow(clientId, "RedirectToAction", "ConsentIndex"));
        }

        private void InitializeFakeObjects()
        {
            _processAuthorizationRequestFake = new Mock<IProcessAuthorizationRequest>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                _processAuthorizationRequestFake.Object,
                _generateAuthorizationResponseFake.Object,
                _clientValidatorFake.Object,
                _oauthEventSource.Object);
        }
    }
}
