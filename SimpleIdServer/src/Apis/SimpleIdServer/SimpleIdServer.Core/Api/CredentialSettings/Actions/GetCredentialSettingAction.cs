using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.CredentialSettings.Actions
{
    public interface IGetCredentialSettingAction
    {
        Task<CredentialSetting> Execute(string credentialType);
    }

    internal sealed class GetCredentialSettingAction : IGetCredentialSettingAction
    {
        private readonly ICredentialSettingsRepository _credentialSettingsRepository;

        public GetCredentialSettingAction(ICredentialSettingsRepository credentialSettingsRepository)
        {
            _credentialSettingsRepository = credentialSettingsRepository;
        }

        public Task<CredentialSetting> Execute(string credentialType)
        {
            if (string.IsNullOrWhiteSpace(credentialType))
            {
                throw new ArgumentNullException(nameof(credentialType));
            }

            return _credentialSettingsRepository.Get(credentialType); 
        }
    }
}
