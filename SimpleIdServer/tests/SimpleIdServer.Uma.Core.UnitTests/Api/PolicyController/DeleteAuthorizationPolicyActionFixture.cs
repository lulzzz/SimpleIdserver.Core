using Moq;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class DeleteAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;                
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IDeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;
        
        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            IntializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteAuthorizationPolicyAction.Execute(null));
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult((Policy)null));

            // ACT
            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId);

            // ASSERT
            Assert.False(isUpdated);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Exists_Then_True_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            var policy = new Policy();
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult(policy));

            // ACT
            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId);

            // ASSERT
            Assert.True(isUpdated);
        }

        private void IntializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _deleteAuthorizationPolicyAction = new DeleteAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
