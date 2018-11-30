using Moq;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Website.ResourcesController.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Website.ResourcesController.Actions
{
    public class UpdateResourcePermissionsActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private IUpdateResourcePermissionsAction _updateResourcePermissionsAction;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourcePermissionsAction.Execute(null));
            await Assert.ThrowsAsync<BaseUmaException>(() => _updateResourcePermissionsAction.Execute(new UpdateResourcePermissionsParameter
            {

            }));
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult((ResourceSet)null));

            // ACT
            var exception = await Assert.ThrowsAsync<UmaResourceNotFoundException>(() => _updateResourcePermissionsAction.Execute(new UpdateResourcePermissionsParameter
            {
                ResourceId = "resourceid"
            }));

            // ASSERTS
            Assert.NotNull(exception);
        }

		[Fact]
		public async Task When_User_Is_Not_Authorized_Then_Exception_Is_Thrown()
		{
            // ARRANGE
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ResourceSet
			{			
				Id = "resourceid",
                Owner = "owner"
			}));
			
			// ACT
            var exception = await Assert.ThrowsAsync<UmaNotAuthorizedException>(() => _updateResourcePermissionsAction.Execute(new UpdateResourcePermissionsParameter
            {
                ResourceId = "resourceid",
				Subject = "incorrectowner"
            }));
			
			// ASSERT
			Assert.NotNull(exception);
		}
		
        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _updateResourcePermissionsAction = new UpdateResourcePermissionsAction(_resourceSetRepositoryStub.Object,
                _policyRepositoryStub.Object);
        }
    }
}