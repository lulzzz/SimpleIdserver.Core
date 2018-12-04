using Moq;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Policies;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Policies
{
    public class AuthorizationPolicyValidatorFixture
    {
        private Mock<IBasicAuthorizationPolicy> _basicAuthorizationPolicyStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IAuthorizationPolicyValidator _authorizationPolicyValidator;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized(null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized("openid", new Ticket(), null));
        }

        [Fact]
        public async Task When_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var ticket = new Ticket
            {
                Lines = new List<TicketLine>
                {
                    new TicketLine
                    {
                        ResourceSetId = "resource_set_id"
                    }
                }
                
            };
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ResourceSet)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _authorizationPolicyValidator.IsAuthorized("openid", ticket, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.SomeResourcesDontExist);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Is_Correct_Then_Authorized_Is_Returned()
        {
            // ARRANGE
            var ticket = new Ticket
            {
                Lines = new List<TicketLine>
                {
                    new TicketLine
                    {
                        ResourceSetId = "1"
                    }
                }
            };
            IEnumerable<ResourceSet> resourceSet = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = "1",
                    AuthorizationPolicyIds = new List<string> { "authorization_policy_id" },
                    AuthPolicies = new List<Policy>()
                }
            };
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(resourceSet));
            _basicAuthorizationPolicyStub.Setup(b => b.Execute(It.IsAny<string>(), It.IsAny<ResourceSet>(), It.IsAny<TicketLineParameter>(), It.IsAny<ClaimTokenParameter>()))
                .Returns(Task.FromResult(new ResourceValidationResult
                {
                    IsValid = true
                }));

            // ACT
            var result = await _authorizationPolicyValidator.IsAuthorized("openid", ticket, null);

            // ASSERT
            Assert.True(result.IsValid);
        }

        private void InitializeFakeObjects()
        {
            _basicAuthorizationPolicyStub = new Mock<IBasicAuthorizationPolicy>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _authorizationPolicyValidator = new AuthorizationPolicyValidator(
                _basicAuthorizationPolicyStub.Object,
                _resourceSetRepositoryStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
