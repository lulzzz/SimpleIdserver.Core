using SimpleIdServer.IdentityStore.LDAP.Extensions;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class CredentialSettingsRepository : ICredentialSettingsRepository
    {
        private readonly ILdapHelperFactory _ldapHelperFactory;
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;

        public CredentialSettingsRepository(ILdapHelperFactory ldapHelperFactory, IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _ldapHelperFactory = ldapHelperFactory;
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public Task<IEnumerable<CredentialSetting>> Get()
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntryCollection = ldapHelper.GetAllCredentialSettings();
            var result = new List<CredentialSetting>();
            foreach(SearchResultEntry searchResultEntry in searchResultEntryCollection)
            {
                result.Add(GetCredentialSetting(searchResultEntry));
            }

            return Task.FromResult((IEnumerable<CredentialSetting>)result);
        }

        public Task<CredentialSetting> Get(string type)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntry = ldapHelper.GetCredentialSetting(type);
            if (searchResultEntry == null)
            {
                return Task.FromResult((CredentialSetting)null);
            }

            return Task.FromResult(GetCredentialSetting(searchResultEntry));
        }

        public Task<IEnumerable<CredentialSetting>> Get(IEnumerable<string> types)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultCollection = ldapHelper.GetCredentialSettings(types);
            if (searchResultCollection == null)
            {
                return Task.FromResult((IEnumerable<CredentialSetting>)null);
            }

            var result = new List<CredentialSetting>();
            foreach(SearchResultEntry record in searchResultCollection)
            {
                result.Add(GetCredentialSetting(record));
            }

            return Task.FromResult((IEnumerable<CredentialSetting>)result);
        }

        public Task<bool> Update(CredentialSetting credentialSetting)
        {
            if (credentialSetting == null)
            {
                throw new ArgumentNullException(nameof(credentialSetting));
            }

            var directoryAttributes = new List<DirectoryAttributeModification>
            {
                GetModification(_identityStoreLDAPOptions.Credential.AuthenticationIntervalsInSecondsName, credentialSetting.AuthenticationIntervalsInSeconds.ToString()),
                GetModification(_identityStoreLDAPOptions.Credential.CredentialTypeName, credentialSetting.CredentialType),
                GetModification(_identityStoreLDAPOptions.Credential.ExpiresInName, credentialSetting.ExpiresIn.ToString()),
                GetModification(_identityStoreLDAPOptions.Credential.IsBlockAccountPolicyEnabledName, credentialSetting.IsBlockAccountPolicyEnabled.ToString()),
                GetModification(_identityStoreLDAPOptions.Credential.NumberOfAuthenticationAttemptsName, credentialSetting.NumberOfAuthenticationAttempts.ToString()),
                GetModification(_identityStoreLDAPOptions.Credential.OptionsName, credentialSetting.Options)
            };
            var ldapHelper = _ldapHelperFactory.Build();
            var result = ldapHelper.UpdateCredentialSetting(credentialSetting.CredentialType, directoryAttributes);
            return Task.FromResult(result);
        }

        private CredentialSetting GetCredentialSetting(SearchResultEntry searchResultEntry)
        {
            var result = new CredentialSetting
            {
                AuthenticationIntervalsInSeconds = searchResultEntry.GetInt(_identityStoreLDAPOptions.Credential.AuthenticationIntervalsInSecondsName),
                CredentialType = searchResultEntry.GetString(_identityStoreLDAPOptions.Credential.CredentialTypeName),
                ExpiresIn = searchResultEntry.GetDouble(_identityStoreLDAPOptions.Credential.ExpiresInName),
                IsBlockAccountPolicyEnabled = searchResultEntry.GetBoolean(_identityStoreLDAPOptions.Credential.IsBlockAccountPolicyEnabledName),
                NumberOfAuthenticationAttempts = searchResultEntry.GetInt(_identityStoreLDAPOptions.Credential.NumberOfAuthenticationAttemptsName),
                Options = searchResultEntry.GetString(_identityStoreLDAPOptions.Credential.OptionsName)
            };
            return result;
        }

        private static DirectoryAttributeModification GetModification(string name, string value)
        {
            var result = new DirectoryAttributeModification
            {
                Operation = DirectoryAttributeOperation.Replace,
                Name = name
            };
            result.Add(value);
            return result;
        }
    }
}
