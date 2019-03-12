using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Core.Api.Profile.Actions;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using Xunit;
using SimpleIdServer.IdentityStore.Repositories;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class UnlinkProfileActionFixture
    {
        private Mock<IUserRepository> _resourceOwnerRepositoryStub;
        private Mock<IProfileRepository> _profileRepositoryStub;
        private IUnlinkProfileAction _unlinkProfileAction;

        [Fact]
        public async Task WhenNullParametersArePassedThenExceptionsAreThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _unlinkProfileAction.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _unlinkProfileAction.Execute("localSubject", null));
        }

        [Fact]
        public async Task WhenResourceOwnerDoesntExistThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult((SimpleIdServer.IdentityStore.Models.User)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _unlinkProfileAction.Execute("localSubject", "externalSubject"));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.TheResourceOwnerDoesntExist, exception.Message);
        }

        [Fact]
        public async Task WhenUserNotAuthorizedToUnlinkProfileThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new SimpleIdServer.IdentityStore.Models.User()));
            _profileRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                UserId = "otherSubject"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _unlinkProfileAction.Execute("localSubject", "externalSubject"));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.NotAuthorizedToRemoveTheProfile, exception.Message);

        }

        [Fact]
        public async Task WhenUnlinkProfileThenOperationIsCalled()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new SimpleIdServer.IdentityStore.Models.User()));
            _profileRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                UserId = "localSubject"
            }));

            // ACT
            await _unlinkProfileAction.Execute("localSubject", "externalSubject");

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Remove(It.Is<IEnumerable<string>>(r => r.Contains("externalSubject"))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IUserRepository>();
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _unlinkProfileAction = new UnlinkProfileAction(_resourceOwnerRepositoryStub.Object,
                _profileRepositoryStub.Object);
        }
    }
}
