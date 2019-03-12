using Moq;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Services;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Services
{
    public class BaseAuthenticateResourceOwnerServiceFixture
    {
        private Mock<ICredentialSettingsRepository> _credentialSettingsRepositoryStub;
        private Mock<IUserCredentialRepository> _userCredentialsRepositoryStub;
        private Mock<BaseAuthenticateResourceOwnerService> _baseAuthenticateResourceOwnerServiceStub;

        [Fact]
        public async Task When_Pass_Null_Parameters_Exception_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult((User)null));

            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerUserAccountDoesntExistException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_User_Account_Is_Blocked_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                IsBlocked = true
            }));


            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerUserAccountBlockedException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Too_Many_Retries_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string amr = "amr";
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(s => s.Amr).Returns(amr);
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                IsBlocked = false,
                Credentials = new []
                {
                    new UserCredential
                    {
                        Type = amr,
                        FirstAuthenticationFailureDateTime = DateTime.UtcNow.AddSeconds(-10),
                        NumberOfAttempts = 11
                    }
                }
            }));
            _credentialSettingsRepositoryStub.Setup(c => c.Get(amr)).Returns(Task.FromResult(new CredentialSetting
            {
                AuthenticationIntervalsInSeconds = 2000,
                NumberOfAuthenticationAttempts = 10
            })); 
            
            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerUserTooManyRetryException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Credential_Is_Blocked_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string amr = "amr";
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(s => s.Amr).Returns(amr);
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                IsBlocked = false,
                Credentials = new[]
                {
                    new UserCredential
                    {
                        Type = amr,
                        FirstAuthenticationFailureDateTime = DateTime.UtcNow.AddSeconds(-10),
                        NumberOfAttempts = 1,
                        IsBlocked = true
                    }
                }
            }));
            _credentialSettingsRepositoryStub.Setup(c => c.Get(amr)).Returns(Task.FromResult(new CredentialSetting
            {
                AuthenticationIntervalsInSeconds = 2000,
                NumberOfAuthenticationAttempts = 10
            }));
            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerCredentialBlockedException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Cannot_Authenticate_User_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string amr = "amr";
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(s => s.Amr).Returns(amr);
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                IsBlocked = false,
                Credentials = new[]
                {
                    new UserCredential
                    {
                        Type = amr,
                        FirstAuthenticationFailureDateTime = DateTime.UtcNow.AddSeconds(-10),
                        NumberOfAttempts = 1,
                        IsBlocked = false
                    }
                }
            }));
            _credentialSettingsRepositoryStub.Setup(c => c.Get(amr)).Returns(Task.FromResult(new CredentialSetting
            {
                AuthenticationIntervalsInSeconds = 2000,
                NumberOfAuthenticationAttempts = 10,
                IsBlockAccountPolicyEnabled = false
            }));
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.Authenticate(It.IsAny<User>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            // ACT
            var ex = await Assert.ThrowsAsync<IdentityServerUserPasswordInvalidException>(() => _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Authenticate_User_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            const string amr = "amr";
            InitializeFakeObjects();
            _baseAuthenticateResourceOwnerServiceStub.Setup(s => s.Amr).Returns(amr);
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                IsBlocked = false,
                Credentials = new[]
                {
                    new UserCredential
                    {
                        Type = amr,
                        FirstAuthenticationFailureDateTime = DateTime.UtcNow.AddSeconds(-10),
                        NumberOfAttempts = 1,
                        IsBlocked = false
                    }
                }
            }));
            _credentialSettingsRepositoryStub.Setup(c => c.Get(amr)).Returns(Task.FromResult(new CredentialSetting
            {
                AuthenticationIntervalsInSeconds = 2000,
                NumberOfAuthenticationAttempts = 10,
                IsBlockAccountPolicyEnabled = false
            }));
            _baseAuthenticateResourceOwnerServiceStub.Setup(b => b.Authenticate(It.IsAny<User>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // ACT
            var ro = await _baseAuthenticateResourceOwnerServiceStub.Object.AuthenticateUserAsync("login", "cred").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ro);
        }

        private void InitializeFakeObjects()
        {
            _credentialSettingsRepositoryStub = new Mock<ICredentialSettingsRepository>();
            _userCredentialsRepositoryStub = new Mock<IUserCredentialRepository>();
            var mock = new Mock<BaseAuthenticateResourceOwnerService>(MockBehavior.Loose, _credentialSettingsRepositoryStub.Object, _userCredentialsRepositoryStub.Object);
            mock.CallBase = true;
            _baseAuthenticateResourceOwnerServiceStub = mock;
        }
    }
}
