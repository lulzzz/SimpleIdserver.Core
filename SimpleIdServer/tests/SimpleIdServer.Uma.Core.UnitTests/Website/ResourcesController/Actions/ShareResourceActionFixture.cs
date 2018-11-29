using Moq;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Website.ResourcesController.Actions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Website.ResourcesController.Actions
{
    public class ShareResourceActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ISharedLinkRepository> _sharedLinkRepositoryStub;
        private IShareResourceAction _shareResourceAction;


        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _shareResourceAction.Execute(null));
            await Assert.ThrowsAsync<BaseUmaException>(() => _shareResourceAction.Execute(new Parameters.ShareResourceParameter
            {

            }));
            await Assert.ThrowsAsync<BaseUmaException>(() => _shareResourceAction.Execute(new Parameters.ShareResourceParameter
            {
                ResourceId = "resourceid"
            }));
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult((ResourceSet)null));

            // ACT
            var exception = await Assert.ThrowsAsync<UmaResourceNotFoundException>(() => _shareResourceAction.Execute(new Parameters.ShareResourceParameter
            {
                ResourceId = "resourceid",
                Scopes = new List<string>
                {
                    "scope"
                }
            }));

            // ASSERTS
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_At_Least_One_Scope_Is_Invalid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope1"
                }
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _shareResourceAction.Execute(new Parameters.ShareResourceParameter
            {
                ResourceId = "resourceid",
                Scopes = new List<string>
                {
                    "scope"
                }
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheScopeAreNotValid, exception.Message);
        }

        [Fact]
        public async Task When_Not_The_ResourceOwner_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope1"
                },
                Owner = "owner"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<UmaNotAuthorizedException>(() => _shareResourceAction.Execute(new Parameters.ShareResourceParameter
            {
                ResourceId = "resourceid",
                Scopes = new List<string>
                {
                    "scope1"
                },
                Owner = "invalidowner"
            }));

            // ASSERTS
            Assert.NotNull(exception);
        }

        private void InitializeFakeObjects()
        {
            _sharedLinkRepositoryStub = new Mock<ISharedLinkRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _shareResourceAction = new ShareResourceAction(_resourceSetRepositoryStub.Object, _sharedLinkRepositoryStub.Object);
        }
    }
}
