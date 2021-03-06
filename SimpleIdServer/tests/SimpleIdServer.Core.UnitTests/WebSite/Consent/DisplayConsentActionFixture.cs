﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.WebSite.Consent.Actions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Consent
{
    public sealed class DisplayConsentActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IClientRepository> _clientRepositoryFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private IDisplayConsentAction _displayConsentAction;

        [Fact]
        public async Task When_Parameter_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _displayConsentAction.Execute(
                null, 
                null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _displayConsentAction.Execute(
                authorizationParameter,
                null, null));
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
        {
            // ARRANGE
            InitializeFakeObjects();
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseMode = ResponseMode.fragment
            };
            var consent = new SimpleIdServer.Core.Common.Models.Consent();
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

            // ACT
            var result = await _displayConsentAction.Execute(authorizationParameter, "subject", null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
            Assert.True(result.ActionResult.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var responseTypes = new List<ResponseType>();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                ResponseMode = ResponseMode.None // No response mode is defined
            };
            var consent = new SimpleIdServer.Core.Common.Models.Consent();
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(responseTypes);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
               "subject", null));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);

        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state
            };
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((SimpleIdServer.Core.Common.Models.Consent)null));
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>())).
                Returns(Task.FromResult((Client)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
               "subject", null));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            const string scopeName = "profile";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Claims = null,
                Scope = scopeName
            };
            ICollection<Scope> scopes = new List<Scope> { new Scope
            {
                IsDisplayedInConsent = true,
                Name = scopeName
            } };
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((SimpleIdServer.Core.Common.Models.Consent)null));
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>())).
                Returns(Task.FromResult(client));
            _scopeRepositoryFake.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT
            await _displayConsentAction.Execute(authorizationParameter,
               "subject", null);

            // ASSERTS
            Assert.True(scopes.Any(s => s.Name == scopeName));
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithOutput());
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _displayConsentAction = new DisplayConsentAction(
                _scopeRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object);
        }
    }
}
