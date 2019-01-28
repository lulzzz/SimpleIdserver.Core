﻿using Moq;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Services
{
    public class BaseAuthenticateResourceOwnerServiceFixture
    {
        private Mock<ICredentialSettingsRepository> _credentialSettingsRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<BaseAuthenticateResourceOwnerService> _baseAuthenticateResourceOwnerServiceStub;

        [Fact]
        public async Task When_Pass_Null_Parameters_Exception_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateResourceOwnerAsync(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateResourceOwnerAsync("login", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetResourceOwner(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerUserAccountDoesntExistException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateResourceOwnerAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        private void InitializeFakeObjects()
        {
            _credentialSettingsRepositoryStub = new Mock<ICredentialSettingsRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            var mock = new Mock<BaseAuthenticateResourceOwnerService>(MockBehavior.Loose, _credentialSettingsRepositoryStub.Object, _resourceOwnerRepositoryStub.Object);
            mock.CallBase = true;
            _baseAuthenticateResourceOwnerServiceStub = mock;
        }
    }
}
