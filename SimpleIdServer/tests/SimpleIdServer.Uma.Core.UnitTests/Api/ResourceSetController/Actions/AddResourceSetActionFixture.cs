using Moq;
using SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Validators;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class AddResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IResourceSetParameterValidator> _resourceSetParameterValidatorStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IAddResourceSetAction _addResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null));
        }

        [Fact]
        public async Task When_Resource_Set_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(() => Task.FromResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetCannotBeInserted);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_Id_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _addResourceSetAction.Execute(addResourceParameter);

            // ASSERTS
            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetParameterValidatorStub = new Mock<IResourceSetParameterValidator>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _addResourceSetAction = new AddResourceSetAction(
                _resourceSetRepositoryStub.Object,
                _resourceSetParameterValidatorStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
