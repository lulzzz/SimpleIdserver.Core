﻿using Moq;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class DeleteResourcePolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IUmaServerEventSource> _umaServerEventSource;
        private IDeleteResourcePolicyAction _deleteResourcePolicyAction;

        [Fact]
        public async Task When_Passing_NullOrEmpty_Parameters_Then_Exceptions_Are_Throwns()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute("policy_id", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute("policy_id", string.Empty));
        }

        [Fact]
        public async Task When_ResourceDoesntExist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult(new Policy()));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(() => Task.FromResult((ResourceSet)null));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
        }

        [Fact]
        public async Task When_PolicyDoesntContainResource_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "invalid_resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult(new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        "resource_id"
                    }
                }));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(() => Task.FromResult(new ResourceSet()));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == ErrorDescriptions.ThePolicyDoesntContainResource);
        }

        [Fact]
        public async Task When_AuthorizationPolicyDoesntExist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult((Policy)null));

            // ACT
            var result = await _deleteResourcePolicyAction.Execute(policyId, "resource_id");

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceIsRemovedFromPolicy_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult(new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        resourceId
                    }
                }));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(() => Task.FromResult(new ResourceSet()));
            _policyRepositoryStub.Setup(p => p.Update(It.IsAny<Policy>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _deleteResourcePolicyAction.Execute(policyId, resourceId);

            // ASSERTS
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _umaServerEventSource = new Mock<IUmaServerEventSource>();
            _deleteResourcePolicyAction = new DeleteResourcePolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _resourceSetRepositoryStub.Object,
                _umaServerEventSource.Object);
        }
    }
}
