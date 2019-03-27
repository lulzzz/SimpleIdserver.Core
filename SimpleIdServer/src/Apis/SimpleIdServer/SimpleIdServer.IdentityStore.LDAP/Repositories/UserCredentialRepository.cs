using SimpleIdServer.IdentityStore.LDAP.Extensions;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class UserCredentialRepository : IUserCredentialRepository
    {
        private readonly ILdapHelperFactory _ldapHelperFactory;
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;

        public UserCredentialRepository(ILdapHelperFactory ldapHelperFactory, IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _ldapHelperFactory = ldapHelperFactory;
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public Task<bool> Add(IEnumerable<UserCredential> userCredentials)
        {
            if (userCredentials == null)
            {
                throw new ArgumentNullException(nameof(userCredentials));
            }

            var ldapHelper = _ldapHelperFactory.Build();
            if (userCredentials != null)
            {
                foreach(var userCredential in userCredentials)
                {
                    var directoryAttributes = new List<DirectoryAttribute>();
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.BlockedDateTimeName, userCredential.BlockedDateTime.ToString()));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.ExpirationDateTimeName, userCredential.ExpirationDateTime.ToString()));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.FirstAuthenticationFailureDateTimeName, userCredential.FirstAuthenticationFailureDateTime == null ? string.Empty : userCredential.FirstAuthenticationFailureDateTime.Value.ToString()));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.IsBlockedName, userCredential.IsBlocked.ToString()));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.NumberOfAttemptsName, userCredential.NumberOfAttempts.ToString()));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.TypeName, userCredential.Type));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.UserIdName, userCredential.UserId));
                    directoryAttributes.Add(GetAdd(_identityStoreLDAPOptions.UserCredential.ValueName, userCredential.Value.ToString()));
                    ldapHelper.InsertUserCredential(userCredential.UserId, userCredential.Type, directoryAttributes);
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> Delete(string subject, string type)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            return Task.FromResult(ldapHelper.DeleteUserCredential(subject, type));
        }

        public Task<UserCredential> Get(string type, string value)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntry = ldapHelper.Get($"(&({_identityStoreLDAPOptions.UserCredential.TypeName}={type})({_identityStoreLDAPOptions.UserCredential.ValueName}={value}))");
            if (searchResultEntry == null)
            {
                return Task.FromResult((UserCredential)null);
            }

            return Task.FromResult(GetUserCredential(searchResultEntry));
        }

        public Task<UserCredential> GetUserCredential(string subject, string type)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntry = ldapHelper.GetUserCredential(subject, type);
            if (searchResultEntry == null)
            {
                return Task.FromResult((UserCredential)null);
            }

            return Task.FromResult(GetUserCredential(searchResultEntry));
        }

        public Task<bool> Update(UserCredential resourceOwnerCredential)
        {
            if (resourceOwnerCredential == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerCredential));
            }

            var directoryAttributes = new List<DirectoryAttributeModification>
            {
                GetModification(_identityStoreLDAPOptions.UserCredential.BlockedDateTimeName, resourceOwnerCredential.BlockedDateTime.ToString()),
                GetModification(_identityStoreLDAPOptions.UserCredential.ExpirationDateTimeName, resourceOwnerCredential.ExpirationDateTime.ToString()),
                GetModification(_identityStoreLDAPOptions.UserCredential.FirstAuthenticationFailureDateTimeName, resourceOwnerCredential.FirstAuthenticationFailureDateTime == null ? string.Empty : resourceOwnerCredential.FirstAuthenticationFailureDateTime.Value.ToString()),
                GetModification(_identityStoreLDAPOptions.UserCredential.IsBlockedName, resourceOwnerCredential.IsBlocked.ToString()),
                GetModification(_identityStoreLDAPOptions.UserCredential.ValueName, resourceOwnerCredential.Value),
            };
            var ldapHelper = _ldapHelperFactory.Build();
            var result = ldapHelper.UpdateUserCredential(resourceOwnerCredential.UserId, resourceOwnerCredential.Type, directoryAttributes);
            return Task.FromResult(result);
        }

        private UserCredential GetUserCredential(SearchResultEntry searchResultEntry)
        {
            var result = new UserCredential
            {
                BlockedDateTime = searchResultEntry.GetDateTime(_identityStoreLDAPOptions.UserCredential.BlockedDateTimeName),
                ExpirationDateTime = searchResultEntry.GetDateTime(_identityStoreLDAPOptions.UserCredential.ExpirationDateTimeName),
                FirstAuthenticationFailureDateTime = searchResultEntry.GetNullableDateTime(_identityStoreLDAPOptions.UserCredential.FirstAuthenticationFailureDateTimeName),
                IsBlocked = searchResultEntry.GetBoolean(_identityStoreLDAPOptions.UserCredential.IsBlockedName),
                NumberOfAttempts = searchResultEntry.GetInt(_identityStoreLDAPOptions.UserCredential.NumberOfAttemptsName),
                Type = searchResultEntry.GetString(_identityStoreLDAPOptions.UserCredential.TypeName),
                UserId = searchResultEntry.GetString(_identityStoreLDAPOptions.UserCredential.UserIdName),
                Value = searchResultEntry.GetString(_identityStoreLDAPOptions.UserCredential.ValueName)
            };

            return result;
        }

        private static DirectoryAttribute GetAdd(string name, string value)
        {
            var result = new DirectoryAttribute
            {
                Name = name
            };
            result.Add(value);
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
