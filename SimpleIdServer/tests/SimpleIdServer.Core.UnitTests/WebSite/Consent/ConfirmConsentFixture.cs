using Moq;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.WebSite.Consent.Actions;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Consent
{
    public sealed class ConfirmConsentFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;
        private Mock<IClientRepository> _clientRepositoryFake;
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IOpenIdEventSource> _openIdEventSource;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;

        private IConfirmConsentAction _confirmConsentAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmConsentAction.Execute(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmConsentAction.Execute(authorizationParameter, null, null));
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_ResponseMode_Is_No_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null,
                Scope = "profile",
                ResponseMode = ResponseMode.None,
                State = state
            };
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new SimpleIdServer.IdentityStore.Models.User
            {
                Id = subject
            };
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction()
            };
            ICollection<string> scopeNames = new List<string>();
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((SimpleIdServer.Core.Common.Models.Consent)null));
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(scopeNames);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            _scopeRepositoryFake.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType> { ResponseType.id_token, ResponseType.id_token });

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _confirmConsentAction.Execute(authorizationParameter, subject, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_For_The_Claims_Then_Create_And_Insert_A_New_One()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = new ClaimsParameter
                {
                    UserInfo = new List<ClaimParameter>
                    {
                        new ClaimParameter
                        {
                            Name = Constants.StandardResourceOwnerClaimNames.Subject
                        }
                    }
                },
                Scope = "profile"
            };
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var client = new Client
            {
                ClientId = clientId
            };
            var resourceOwner = new SimpleIdServer.IdentityStore.Models.User
            {
                Id = subject
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((SimpleIdServer.Core.Common.Models.Consent)null));
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(new List<string>());
            _scopeRepositoryFake.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            SimpleIdServer.Core.Common.Models.Consent insertedConsent = null;
            _consentRepositoryFake.Setup(co => co.InsertAsync(It.IsAny<SimpleIdServer.Core.Common.Models.Consent>()))
                .Callback<SimpleIdServer.Core.Common.Models.Consent>(consent => insertedConsent = consent)
                .Returns(Task.FromResult(true));

            // ACT
            await _confirmConsentAction.Execute(authorizationParameter, subject, null);

            // ASSERT
            Assert.NotNull(insertedConsent);
            Assert.True(insertedConsent.Claims.Contains(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(insertedConsent.UserId == subject);
            Assert.True(insertedConsent.Client.ClientId == clientId);
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null,
                Scope = "profile",
                ResponseMode = ResponseMode.None
            };
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new SimpleIdServer.IdentityStore.Models.User
            {
                Id = subject
            };
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction
                {
                    
                }
            };
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((SimpleIdServer.Core.Common.Models.Consent)null));
            _clientRepositoryFake.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(new List<string>());
            _scopeRepositoryFake.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType> {  ResponseType.code });

            // ACT
            var result = await _confirmConsentAction.Execute(authorizationParameter, subject, null);

            // ASSERT
            _consentRepositoryFake.Verify(c => c.InsertAsync(It.IsAny<SimpleIdServer.Core.Common.Models.Consent>()));
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
            Assert.True(result.RedirectInstruction.ResponseMode == ResponseMode.query);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _openIdEventSource = new Mock<IOpenIdEventSource>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _confirmConsentAction = new ConfirmConsentAction(
                _consentRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _scopeRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _generateAuthorizationResponseFake.Object,
                _consentHelperFake.Object,
                _openIdEventSource.Object);
        }
    }
}
