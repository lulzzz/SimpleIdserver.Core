using Moq;
using SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class GetAllResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private IGetAllResourceSetAction _getAllResourceSetAction;

        [Fact]
        public async Task When_Error_Occured_While_Trying_To_Retrieve_ResourceSet_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            _resourceSetRepositoryStub.Setup(r => r.GetAll())
                .Returns(() => Task.FromResult((ICollection<ResourceSet>)null));

            // ACT & ASSERTS
            var exception  = await Assert.ThrowsAsync<BaseUmaException>(() => _getAllResourceSetAction.Execute());
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetsCannotBeRetrieved);
        }

        [Fact]
        public async Task When_ResourceSets_Are_Retrieved_Then_Ids_Are_Returned()
        {
            // ARRANGE
            const string id = "id";
            ICollection<ResourceSet> resourceSets = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = id
                }
            };
            InitializeFakeObject();
            _resourceSetRepositoryStub.Setup(r => r.GetAll())
                .Returns(Task.FromResult(resourceSets));

            // ACT
            var result = await _getAllResourceSetAction.Execute();

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == id);
        }

        private void InitializeFakeObject()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _getAllResourceSetAction = new GetAllResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
