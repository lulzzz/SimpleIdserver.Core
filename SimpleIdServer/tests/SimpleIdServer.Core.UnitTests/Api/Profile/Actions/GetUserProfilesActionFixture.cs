using Moq;
using SimpleIdServer.Core.Api.Profile.Actions;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class GetUserProfilesActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IUserRepository> _resourceOwnerRepositoryStub;
        private IGetUserProfilesAction _getProfileAction;

        [Fact]
        public async Task WhenPassingNullParameterThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(string.Empty));
        }

        [Fact]
        public async Task When_User_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getProfileAction.Execute("subject"));
            Assert.NotNull(ex);
            Assert.Equal("internal_error", ex.Code);
            Assert.Equal("the resource owner doesn't exist", ex.Message);
        }

        [Fact]
        public async Task WhenGetProfileThenOperationIsCalled()
        {
            const string subject = "subject";
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new SimpleIdServer.IdentityStore.Models.User()));

            // ACT
            await _getProfileAction.Execute(subject);

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Search(It.Is<SearchProfileParameter>(r => r.ResourceOwnerIds.Contains(subject))));
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _resourceOwnerRepositoryStub = new Mock<IUserRepository>();
            _getProfileAction = new GetUserProfilesAction(_profileRepositoryStub.Object, _resourceOwnerRepositoryStub.Object);
        }
    }
}
