﻿using Moq;
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
    public class AddResourceSetToPolicyActionFixture
    {        
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IAddResourceSetToPolicyAction _addResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null));
        }

        [Fact]
        public async Task When_Passing_NoPolicyId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter()));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddResourceSetParameterNames.PolicyId));
        }

        [Fact]
        public async Task When_Passing_NoResourceSetIds_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = "policy_id"
            }));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddResourceSetParameterNames.ResourceSet));
        }

        [Fact]
        public async Task When_One_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult(new Policy()));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(Task.FromResult((ResourceSet)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    resourceSetId
                }
            }));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult((Policy)null));

            // ACT
            var result = await _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    "resource_set_id"
                }
            });

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_True_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult(new Policy
                {
                    ResourceSetIds = new List<string>()
                }));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(() => Task.FromResult(new ResourceSet()));
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(ErrorDescriptions.ThePolicyCannotBeUpdated, It.IsAny<Func<Task<bool>>>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    resourceSetId
                }
            });

            // ASSERT
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _addResourceSetAction = new AddResourceSetToPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
