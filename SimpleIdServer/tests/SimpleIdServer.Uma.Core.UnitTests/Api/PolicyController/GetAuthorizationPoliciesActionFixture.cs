using Moq;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class GetAuthorizationPoliciesActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelper;
        private IGetAuthorizationPoliciesAction _getAuthorizationPoliciesAction;

        [Fact]
        public async Task When_Getting_Authorization_Policies_Then_A_ListIds_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            InitializeFakeObjects();
            ICollection<Policy> policies = new List<Policy>
            {
                new Policy
                {
                    Id = policyId
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved,
                It.IsAny<Func<Task<ICollection<Policy>>>>()))
                .Returns(Task.FromResult(policies));

            // ACT
            var result = await _getAuthorizationPoliciesAction.Execute();

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == policyId);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelper = new Mock<IRepositoryExceptionHelper>();
            _getAuthorizationPoliciesAction = new GetAuthorizationPoliciesAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelper.Object);
        }
    }
}
