using Moq;
using SimpleIdServer.Core.Api.User.Actions;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.User
{
    public class UpdateUserClaimsOperationFixture
    {
        private Mock<IUserRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IUpdateUserClaimsOperation _updateUserClaimsOperation;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute("subject", null));
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult((SimpleIdServer.IdentityStore.Models.User)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _updateUserClaimsOperation.Execute("subject", new List<ClaimAggregate>()));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new SimpleIdServer.IdentityStore.Models.User
                {
                    Claims = new List<Claim>
                    {
                        new Claim("type", "value"),
                        new Claim("type1", "value")
                    }
                }));
            _claimRepositoryStub.Setup(r => r.GetAllAsync()).Returns(Task.FromResult((IEnumerable<ClaimAggregate>)new List<ClaimAggregate>
            {
                new ClaimAggregate
                {
                    Code = "type"
                }
            }));

            // ACT
            await _updateUserClaimsOperation.Execute("subjet", new List<ClaimAggregate>
            {
                new ClaimAggregate("type", "value1")
            });

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(p => p.UpdateAsync(It.Is<SimpleIdServer.IdentityStore.Models.User>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1"))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IUserRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _updateUserClaimsOperation = new UpdateUserClaimsOperation(_resourceOwnerRepositoryStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
