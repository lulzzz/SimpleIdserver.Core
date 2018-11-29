using Moq;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Website.ResourcesController.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Website.ResourcesController.Actions
{
    public class ConfirmSharedLinkActionFixture
    {
        private Mock<ISharedLinkRepository> _sharedLinkRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private IConfirmSharedLinkAction _confirmSharedLinkAction;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmSharedLinkAction.Execute(null));
            await Assert.ThrowsAsync<BaseUmaException>(() => _confirmSharedLinkAction.Execute(new Parameters.ConfirmSharedLinkParameter
            {

            }));
        }

        [Fact]
        public async Task When_ConfirmationCode_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _sharedLinkRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult((SharedLink)null));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _confirmSharedLinkAction.Execute(new Parameters.ConfirmSharedLinkParameter
            {
                ConfirmationCode = "confirmationcode"
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheConfirmationCodeIsInvalid, exception.Message);
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _sharedLinkRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new SharedLink
            {
                ConfirmationCode = "confirmationcode",
                ResourceId = "resourceid"
            }));
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult((ResourceSet) null));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _confirmSharedLinkAction.Execute(new Parameters.ConfirmSharedLinkParameter
            {
                ConfirmationCode = "confirmationcode"
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
        }

        [Fact]
        public async Task When_ResourceOwner_Is_The_Same_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _sharedLinkRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new SharedLink
            {
                ConfirmationCode = "confirmationcode",
                ResourceId = "resourceid"
            }));
            _resourceSetRepositoryStub.Setup(s => s.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ResourceSet
            {
                Id = "resourceid",
                Owner = "owner"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _confirmSharedLinkAction.Execute(new Parameters.ConfirmSharedLinkParameter
            {
                ConfirmationCode = "confirmationcode",
                Subject = "owner"
            }));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheSharedLinkCannotBeUsedByTheOwner, exception.Message);
        }

        private void InitializeFakeObjects()
        {
            _sharedLinkRepositoryStub = new Mock<ISharedLinkRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _confirmSharedLinkAction = new ConfirmSharedLinkAction(_sharedLinkRepositoryStub.Object,
                _resourceSetRepositoryStub.Object,
                _policyRepositoryStub.Object);
        }
    }
}