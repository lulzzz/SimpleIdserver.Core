using SimpleIdServer.Core.Api.CredentialSettings.Actions;
using SimpleIdServer.Core.Common.Models;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.CredentialSettings
{
    public interface ICredentialSettingActions
    {
        Task<CredentialSetting> Get(string credentialType);
    }

    internal sealed class CredentialSettingActions : ICredentialSettingActions
    {
        private readonly IGetCredentialSettingAction _getCredentialSettingAction;

        public CredentialSettingActions(IGetCredentialSettingAction getCredentialSettingAction)
        {
            _getCredentialSettingAction = getCredentialSettingAction;
        }

        public Task<CredentialSetting> Get(string credentialType)
        {
            return _getCredentialSettingAction.Execute(credentialType);
        }
    }
}
