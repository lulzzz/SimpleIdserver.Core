using Moq;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.WebSite.Authenticate.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class ChangePasswordActionFixture
    {
        private Mock<ICredentialSettingsRepository> _passwordSettingsRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IChangePasswordAction _changePasswordAction;

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _changePasswordAction.Execute(null));
        }

        [Fact]
        public async Task When_Resource_Owner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _changePasswordAction.Execute(new ChangePasswordParameter()));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(SimpleIdServer.Core.Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(SimpleIdServer.Core.Errors.ErrorDescriptions.TheResourceOwnerDoesntExist, exception.Message);
        }

        [Fact]
        public async Task When_Resource_Owner_Credentials_Are_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Password = "pass"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _changePasswordAction.Execute(new ChangePasswordParameter
            {
                ActualPassword = "incorrectpass"
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(SimpleIdServer.Core.Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(SimpleIdServer.Core.Errors.ErrorDescriptions.ThePasswordIsNotCorrect, exception.Message);
        }

        [Fact]
        public async Task When_RegularExpression_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string pass = "pass";
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Password = PasswordHelper.ComputeHash(pass)
            }));
            _passwordSettingsRepositoryStub.Setup(p => p.Get()).Returns(Task.FromResult(new PasswordSettings
            {
                IsRegexEnabled = true,
                RegularExpression = @"^(?=(.*\d){2})(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\d]).{8,}$",
                PasswordDescription = "invalid"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _changePasswordAction.Execute(new ChangePasswordParameter
            {
                ActualPassword = pass,
                NewPassword = "newpass"
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(SimpleIdServer.Core.Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(SimpleIdServer.Core.Errors.ErrorDescriptions.ThePasswordMustRespects, "invalid"), exception.Message);
        }

        private void InitializeFakeObjects()
        {
            _passwordSettingsRepositoryStub = new Mock<ICredentialSettingsRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _changePasswordAction = new ChangePasswordAction(_passwordSettingsRepositoryStub.Object, _resourceOwnerRepositoryStub.Object);
        }
    }
}
