using Moq;
using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Repositories;
using Xunit;
using SimpleIdServer.Core.Api.User.Actions;

namespace SimpleIdentityServer.Core.UnitTests.Api.User
{
    public class RemoveConsentOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;
        private IRemoveConsentOperation _removeConsentOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT && ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _removeConsentOperation.Execute(null));
        }

        [Fact]
        public async Task When_Deleting_Consent_Then_Boolean_Is_Returned()
        {
            // ARRANGE
            const bool isRemoved = true;
            const string consentId = "consent_id";
            InitializeFakeObjects();
            _consentRepositoryStub.Setup(c => c.DeleteAsync(It.IsAny<SimpleIdServer.Core.Common.Models.Consent>()))
                .Returns(Task.FromResult(isRemoved));

            // ACT
            var result = await _removeConsentOperation.Execute(consentId);

            // ASSERT
            Assert.True(result == isRemoved);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _removeConsentOperation = new RemoveConsentOperation(_consentRepositoryStub.Object);
        }
    }
}
