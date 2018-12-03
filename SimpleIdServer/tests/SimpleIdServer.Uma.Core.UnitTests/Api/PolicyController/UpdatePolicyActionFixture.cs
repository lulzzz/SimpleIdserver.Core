using Moq;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class UpdatePolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IUpdatePolicyAction _updatePolicyAction;
        
        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updatePolicyAction.Execute(null));
        }

        [Fact]
        public async Task When_Id_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Task<Policy>>>())).Returns(() => Task.FromResult((Policy)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _updatePolicyAction.Execute(updatePolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
        }
        
        [Fact]
        public async Task When_Authorization_Policy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
                PolicyId = "not_valid_policy_id"
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Task<Policy>>>())).Returns(() => Task.FromResult((Policy)null));

            // ACT
            var result = await _updatePolicyAction.Execute(updatePolicyParameter);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
                PolicyId = "policy_id",
                Scopes = new List<string>
                {
                    "invalid_scope"
                }
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Task<Policy>>>())).Returns(() => Task.FromResult(new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        "resource_id"
                    }
                }));
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            }));

            // ACT
            var result = await Assert.ThrowsAsync<BaseUmaException>(() => _updatePolicyAction.Execute(updatePolicyParameter));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("invalid_scope", result.Code);
            Assert.Equal("one or more scopes don't belong to a resource set", result.Message);
        }

        [Fact]
        public async Task When_Authorization_Policy_Is_Updated_Then_True_Is_Returned()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
                PolicyId = "valid_policy_id",
                Claims = new List<AddClaimParameter>
                {
                    new AddClaimParameter
                    {
                        Type = "type",
                        Value = "value"
                    }
                },
                Scopes = new List<string>
                {
                    "scope"
                }

            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Task<Policy>>>())).Returns(Task.FromResult(new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        "resource_id"
                    }
                }));
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Task<bool>>>())).Returns(Task.FromResult(true));
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            }));

            // ACT
            var result = await _updatePolicyAction.Execute(updatePolicyParameter);

            // ASSERT
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _updatePolicyAction = new UpdatePolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _resourceSetRepositoryStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
