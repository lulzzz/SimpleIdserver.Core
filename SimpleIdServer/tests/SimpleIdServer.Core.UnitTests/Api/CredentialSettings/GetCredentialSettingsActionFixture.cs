using Moq;
using SimpleIdServer.Core.Api.CredentialSettings.Actions;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.CredentialSettings
{
    public class GetCredentialSettingsActionFixture
    {
        private Mock<ICredentialSettingsRepository> _credentialSettingRepositoryStub;
        private IGetCredentialSettingAction _getCredentialSettingAction;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getCredentialSettingAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getCredentialSettingAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_GetCredentialSetting_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _getCredentialSettingAction.Execute("credentialType").ConfigureAwait(false);

            // ASSERT
            _credentialSettingRepositoryStub.Verify(c => c.Get("credentialType"));
        }

        private void InitializeFakeObjects()
        {
            _credentialSettingRepositoryStub = new Mock<ICredentialSettingsRepository>();
            _getCredentialSettingAction = new GetCredentialSettingAction(_credentialSettingRepositoryStub.Object);
        }
    }
}
