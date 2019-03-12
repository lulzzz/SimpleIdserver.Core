using Moq;
using SimpleIdServer.Authenticate.SMS.Actions;
using SimpleIdServer.Core.Api.User;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.IdentityStore.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Authenticate.SMS.Tests.Actions
{
    public class SmsAuthenticationOperationFixture
    {
        private Mock<IGenerateAndSendSmsCodeOperation> _generateAndSendSmsCodeOperationStub;
        private Mock<IUserActions> _userActionsStub;
        private SmsAuthenticationOptions _smsAuthenticationOptions;
        private ISmsAuthenticationOperation _smsAuthenticationOperation;

        [Fact]
        public async Task When_Null_Parameter_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(string.Empty));
        }

        [Fact]
        public async Task When_SelfProvisioningIsDisabled_And_ResourceOwner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string phone = "phone";
            var resourceOwner = new User
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _smsAuthenticationOptions.IsSelfProvisioningEnabled = false;
            _userActionsStub.Setup(p => p.GetUserByClaim("phone_number", phone)).Returns(() => Task.FromResult((User)null));

            // ACT 
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => _smsAuthenticationOperation.Execute(phone));

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async Task When_ResourceOwner_Exists_Then_ResourceOwner_Is_Returned()
        {
            // ARRANGE
            const string phone = "phone";
            var resourceOwner = new User
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _userActionsStub.Setup(p => p.GetUserByClaim("phone_number", phone)).Returns(() => Task.FromResult(resourceOwner));

            // ACT
            var result = await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            Assert.NotNull(result);
            Assert.Equal(resourceOwner.Id, result.Id);
        }

        [Fact]
        public async Task When_SelfProvisioningIsEnabled_And_ResourceOwnerDoesntExist_Then_NewOne_Is_Created()
        {
            // ARRANGE
            const string phone = "phone";
            InitializeFakeObjects();
            _smsAuthenticationOptions.IsSelfProvisioningEnabled = true;
            _userActionsStub.Setup(p => p.GetUserByClaim("phone", phone)).Returns(() => Task.FromResult((User)null));
            
            // ACT
            await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            _userActionsStub.Verify(u => u.AddUser(It.IsAny<AddUserParameter>(), It.IsAny<string>()));
        }

        private void InitializeFakeObjects()
        {
            _generateAndSendSmsCodeOperationStub = new Mock<IGenerateAndSendSmsCodeOperation>();
            _userActionsStub = new Mock<IUserActions>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions();
            _smsAuthenticationOperation = new SmsAuthenticationOperation(_generateAndSendSmsCodeOperationStub.Object, _userActionsStub.Object, _smsAuthenticationOptions);
        }
    }
}
