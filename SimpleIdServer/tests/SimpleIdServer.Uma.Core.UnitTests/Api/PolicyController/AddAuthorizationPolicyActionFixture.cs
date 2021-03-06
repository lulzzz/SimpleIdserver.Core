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
    public class AddAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelper;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IAddAuthorizationPolicyAction _addAuthorizationPolicyAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthorizationPolicyAction.Execute(null));
        }

        [Fact]
        public async Task When_Passing_Empty_ResourceSetId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addPolicyParameter = new AddPolicyParameter();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.ResourceSetIds));
        }

        [Fact]
        public async Task When_ResourceSetId_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Scopes = new List<string>
                {
                    "invalid_scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(Task.FromResult((ResourceSet)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Scopes = new List<string>
                {
                    "invalid_scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(Task.FromResult(resourceSet));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
        }

        [Fact]
        public async Task When_Adding_AuthorizationPolicy_Then_Id_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Scopes = new List<string>
                {
                    "scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                },
                Claims = new List<AddClaimParameter>
                {
                    new AddClaimParameter
                    {
                        Type = "type",
                        Value = "value"
                    }
                }

            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<Task<ResourceSet>>>()))
                .Returns(Task.FromResult(resourceSet));

            // ACT
            var result = await _addAuthorizationPolicyAction.Execute(addPolicyParameter);

            // ASSERTS
            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _repositoryExceptionHelper = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _addAuthorizationPolicyAction = new AddAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object,
                _repositoryExceptionHelper.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
