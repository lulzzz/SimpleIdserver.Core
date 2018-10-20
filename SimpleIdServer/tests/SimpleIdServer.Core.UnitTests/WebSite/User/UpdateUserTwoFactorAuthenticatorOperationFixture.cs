using Moq;
using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.WebSite.User.Actions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class UpdateUserTwoFactorAuthenticatorOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IUpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute(null, null));
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor"));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor");

            // ASSERTS
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
