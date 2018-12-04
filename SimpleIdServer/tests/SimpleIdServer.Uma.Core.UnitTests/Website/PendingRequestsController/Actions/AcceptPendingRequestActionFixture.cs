using Moq;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Website.PendingRequestsController.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Website.PendingRequestsController.Actions
{
    public class AcceptPendingRequestActionFixture
    {
        private Mock<IPendingRequestRepository> _pendingRequestRepositoryStub;
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private IAcceptPendingRequestAction _rejectPendingRequestAction;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _rejectPendingRequestAction.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _rejectPendingRequestAction.Execute("subject", null));
        }

        [Fact]
        public async Task When_PendingRequest_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _pendingRequestRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult((PendingRequest)null));

            // ACT
            await Assert.ThrowsAsync<UmaPolicyNotFoundException>(() => _rejectPendingRequestAction.Execute("subject", "resourceid"));
        }

        [Fact]
        public async Task When_User_Is_Not_Authorized_To_Accept_Pending_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _pendingRequestRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new PendingRequest
            {
                Resource = new ResourceSet
                {
                    Owner = "owner"
                }
            }));

            // ACT
            await Assert.ThrowsAsync<UmaNotAuthorizedException>(() => _rejectPendingRequestAction.Execute("subject", "resourceid"));
        }

        private void InitializeFakeObjects()
        {
            _pendingRequestRepositoryStub = new Mock<IPendingRequestRepository>();
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _rejectPendingRequestAction = new AcceptPendingRequestAction(_pendingRequestRepositoryStub.Object, _policyRepositoryStub.Object);
        }
    }
}
