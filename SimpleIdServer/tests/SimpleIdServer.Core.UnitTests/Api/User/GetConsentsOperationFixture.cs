using Moq;
using SimpleIdServer.Core.Api.User.Actions;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.User
{
    public class GetConsentsOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;
        private IGetConsentsOperation _getConsentsOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getConsentsOperation.Execute(null));
        }

        [Fact]
        public async Task When_Getting_Consents_A_List_Is_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            InitializeFakeObjects();
            IEnumerable<SimpleIdServer.Core.Common.Models.Consent> consents = new List<SimpleIdServer.Core.Common.Models.Consent>
            {
                new SimpleIdServer.Core.Common.Models.Consent
                {
                    Id = "consent_id"
                }
            };
            _consentRepositoryStub.Setup(c => c.GetConsentsForGivenUserAsync(subject))
                .Returns(Task.FromResult(consents));

            // ACT
            var result = await _getConsentsOperation.Execute(subject);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result == consents);
        }
        
        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _getConsentsOperation = new GetConsentsOperation(_consentRepositoryStub.Object);
        }
    }
}
