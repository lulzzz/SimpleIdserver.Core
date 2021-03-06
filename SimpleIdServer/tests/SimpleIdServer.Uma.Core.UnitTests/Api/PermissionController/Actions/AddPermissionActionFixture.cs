﻿using Moq;
using SimpleIdServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Services;
using SimpleIdServer.Uma.Core.Stores;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.PermissionController.Actions
{
    public class AddPermissionActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ITicketStore> _ticketStoreStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private Mock<IUmaConfigurationService> _configurationServiceStub;
        private IAddPermissionAction _addPermissionAction;

        [Fact]
        public async Task When_Passing_No_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(null, (IEnumerable<AddPermissionParameter>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(new[] { "audience" }, (AddPermissionParameter)null));
        }

        [Fact]
        public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(new[] { "audience" }, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.ResourceSetId));
        }

        [Fact]
        public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = "resource_set_id"
            };

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(new[] { "audience" }, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.Scopes));
        }

        [Fact]
        public async Task When_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult((IEnumerable<ResourceSet>)new List<ResourceSet>()));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(new[] { "audience" }, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public async Task When_Scope_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "invalid_scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult(resources));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(new[] { "audience" }, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeAreNotValid);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {           
            // ARRANGE
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            _ticketStoreStub.Setup(r => r.AddAsync(It.IsAny<Ticket>())).Returns(Task.FromResult(true));
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult(resources));
            _configurationServiceStub.Setup(c => c.GetTicketLifeTime()).Returns(Task.FromResult(2));

            // ACT
            var result = await _addPermissionAction.Execute(new[] { "audience" }, addPermissionParameter);

            // ASSERTS
            Assert.NotEmpty(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _ticketStoreStub = new Mock<ITicketStore>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _configurationServiceStub = new Mock<IUmaConfigurationService>();
            _addPermissionAction = new AddPermissionAction(
                _resourceSetRepositoryStub.Object,
                _ticketStoreStub.Object,
                _repositoryExceptionHelperStub.Object,
                _configurationServiceStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
