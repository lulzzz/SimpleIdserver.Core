﻿using Moq;
using SimpleIdServer.Core.Api.User.Actions;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.User
{
    public class GetUserOperationFixture
    {
        private Mock<IUserRepository> _resourceOwnerRepositoryStub;
        private IGetUserOperation _getUserOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getUserOperation.Execute(null));
        }
        
        [Fact]
        public async Task When_Correct_Subject_Is_Passed_Then_ResourceOwner_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new SimpleIdServer.IdentityStore.Models.User()));

            // ACT
            var result = await _getUserOperation.Execute("subject").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }
        
        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IUserRepository>();
            _getUserOperation = new GetUserOperation(_resourceOwnerRepositoryStub.Object);
        }
    }
}
