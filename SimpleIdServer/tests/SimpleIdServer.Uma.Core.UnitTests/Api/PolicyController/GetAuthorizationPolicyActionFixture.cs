using Moq;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class GetAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private IGetAuthorizationPolicyAction _getAuthorizationPolicyAction;

        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationPolicyAction.Execute(null));
        }
        
        [Fact]
        public async Task When_Getting_Policy_Then_Policy_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            var policy = new Policy
            {
                Id = policyId
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult(policy));

            // ACT
            var result = await _getAuthorizationPolicyAction.Execute(policyId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Id == policyId);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _getAuthorizationPolicyAction = new GetAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object);
        }
    }
}
