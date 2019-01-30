using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using SimpleIdServer.Authenticate.SMS.Services;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Store;
using Xunit;

namespace SimpleIdServer.Authenticate.SMS.Tests.Services
{
    public class SmsAuthenticateResourceOwnerServiceFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("login", null));
        }

        [Fact]
        public async Task When_Subject_Is_Different_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetResourceOwnerByClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("phone_number", "phone")
                },
                Credentials = new List<ResourceOwnerCredential>
                {
                    new ResourceOwnerCredential
                    {
                        Type = "sms"
                    }
                }
            }));
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = "notcorrectphone"
            }));

            // ACT
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("login", "password").ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Expired_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects(); _resourceOwnerRepositoryStub.Setup(r => r.GetResourceOwnerByClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("phone_number", "phone")
                },
                Credentials = new List<ResourceOwnerCredential>
                {
                    new ResourceOwnerCredential
                    {
                        Type = "sms"
                    }
                }
            }));
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = "phone",
                IssueAt = DateTime.UtcNow.AddDays(-1),
                ExpiresIn = 100
            }));

            // ACT
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("phone", "password").ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Correct_And_PhoneNumber_Correct_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetResourceOwnerByClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("phone_number", "phone")
                },
                Credentials = new List<ResourceOwnerCredential>
                {
                    new ResourceOwnerCredential
                    {
                        Type = "sms"
                    }
                }
            }));
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = "phone",
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 100
            }));


            // ACT
            await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("phone", "password");

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, "phone"));
            _confirmationCodeStoreStub.Verify(c => c.Remove("password"));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _authenticateResourceOwnerService = new SmsAuthenticateResourceOwnerService(_confirmationCodeStoreStub.Object, _resourceOwnerRepositoryStub.Object);
        }
    }
}
