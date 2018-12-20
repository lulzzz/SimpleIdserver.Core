using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.WebSite.Authenticate;
using SimpleIdServer.Core.WebSite.Authenticate.Actions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class AuthenticateActionsFixture
    {
        private Mock<IAuthenticateResourceOwnerOpenIdAction> _authenticateResourceOwnerActionFake;
        private Mock<ILocalOpenIdUserAuthenticationAction> _localOpenIdUserAuthenticationActionFake;
        private Mock<IGenerateAndSendCodeAction> _generateAndSendCodeActionStub;
        private Mock<IValidateConfirmationCodeAction> _validateConfirmationCodeActionStub;
        private Mock<IRemoveConfirmationCodeAction> _removeConfirmationCodeActionStub;
        private Mock<IChangePasswordAction> _changePasswordAction;
        private IAuthenticateActions _authenticateActions;

        [Fact]
        public async Task When_Passing_Null_AuthorizationParameter_To_The_Action_AuthenticateResourceOwner_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwnerOpenId(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwnerOpenId(authorizationParameter, null, null, null));
        }

        [Fact]
        public async Task When_Passing_Null_LocalAuthenticateParameter_To_The_Action_LocalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateActions.LocalOpenIdUserAuthentication(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateActions.LocalOpenIdUserAuthentication(localAuthenticationParameter, null, null, null));
        }

        [Fact]
        public async Task When_Passing_Parameters_Needed_To_The_Action_AuthenticateResourceOwner_Then_The_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "clientId"
            };
            var claimsPrincipal = new ClaimsPrincipal();

            // ACT
            await _authenticateActions.AuthenticateResourceOwnerOpenId(authorizationParameter, claimsPrincipal, null, null);

            // ASSERT
            _authenticateResourceOwnerActionFake.Verify(a => a.Execute(authorizationParameter, claimsPrincipal, null, null));
        }

        [Fact]
        public async Task When_Passing_Parameters_Needed_To_The_Action_LocalUserAuthentication_Then_The_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "clientId"
            };
            var localUserAuthentication = new LocalAuthenticationParameter
            {
                UserName = "userName"
            };


            // ACT
            await _authenticateActions.LocalOpenIdUserAuthentication(localUserAuthentication, authorizationParameter, null, null);

            // ASSERT
            _localOpenIdUserAuthenticationActionFake.Verify(a => a.Execute(localUserAuthentication, 
                authorizationParameter,
                null, null));
        }
        
        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerActionFake = new Mock<IAuthenticateResourceOwnerOpenIdAction>();
            _localOpenIdUserAuthenticationActionFake = new Mock<ILocalOpenIdUserAuthenticationAction>();
            _generateAndSendCodeActionStub = new Mock<IGenerateAndSendCodeAction>();
            _validateConfirmationCodeActionStub = new Mock<IValidateConfirmationCodeAction>();
            _removeConfirmationCodeActionStub = new Mock<IRemoveConfirmationCodeAction>();
            _changePasswordAction = new Mock<IChangePasswordAction>();
            _authenticateActions = new AuthenticateActions(
                _authenticateResourceOwnerActionFake.Object,
                _localOpenIdUserAuthenticationActionFake.Object,
                _generateAndSendCodeActionStub.Object,
                _validateConfirmationCodeActionStub.Object,
                _removeConfirmationCodeActionStub.Object,
                _changePasswordAction.Object);
        }
    }
}
