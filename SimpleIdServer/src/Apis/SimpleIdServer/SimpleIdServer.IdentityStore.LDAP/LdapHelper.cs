using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;

namespace SimpleIdServer.IdentityStore.LDAP
{
    public class RootDSE
    {
        public string VendorName { get; set; }
        public string VendorVersion { get; set; }
        public string SupportedLDAPVersion { get; set; }
        public string DefaultNamingContext { get; set; }
        public string SchemaNamingContext { get; set; }
        public string ServerName { get; set; }
        public string SubSchemaSubEntry { get; set; }
        public string ConfigurationNamingContext { get; set; }
        public ICollection<string> NamingContexts { get; set; }
        public ICollection<string> SupportedSASMechanisms { get; set; }
        public ICollection<string> SupportedCapabilities { get; set; }
    }

    public interface ILdapHelper
    {
        RootDSE GetRootDSE();
        LdapConnection AuthenticateAsAdmin();
        LdapConnection AuthenticateBasicLoginAndPassword(string login, string password, string domain = null);
        SearchResultEntryCollection GetAllUsers();
        SearchResultEntryCollection GetAllCredentialSettings();
        SearchResultEntryCollection GetUsers(IEnumerable<string> ids);
        SearchResultEntryCollection GetCredentialSettings(IEnumerable<string> ids);
        SearchResultEntryCollection GetCollection(string filter);
        SearchResultEntryCollection GetPagedUsers(int pageSize, int startIndex);
        SearchResultEntry GetUser(string id);
        SearchResultEntry GetUserCredential(string userId, string type);
        SearchResultEntry GetCredentialSetting(string id);
        SearchResultEntry Get(string filter);
        bool DeleteUser(string userId);
        bool DeleteUserCredential(string userId, string type);
        bool Delete(string distinguishedName);
        bool InsertUserCredential(string userId, string type, ICollection<DirectoryAttribute> directoryAttributes);
        bool InsertUser(string userId, ICollection<DirectoryAttribute> directoryAttributes);
        bool Insert(string distinguishedName, ICollection<DirectoryAttribute> directoryAttributes);
        bool UpdateUser(string userId, ICollection<DirectoryAttributeModification> directoryAttributeModifications);
        bool UpdateUserCredential(string userId, string type, ICollection<DirectoryAttributeModification> directoryAttributeModifications);
        bool UpdateCredentialSetting(string credentialSettingId, ICollection<DirectoryAttributeModification> directoryAttributeModifications);
        bool Update(string distinguishedName, ICollection<DirectoryAttributeModification> directoryAttributeModifications);
    }

    internal class LdapHelper : ILdapHelper
    {
        private Dictionary<string, Action<DirectoryAttribute, RootDSE>> _mapping = new Dictionary<string, Action<DirectoryAttribute, RootDSE>>
        {
            { "supportedldapversion", (da, rdse) =>
            {
                var version = string.Join(".", da.GetValues(typeof(string)));
                rdse.SupportedLDAPVersion = version;
            } },
            { "supportedsaslmechanisms", (da, rdse) =>
            {
                var supportedSASLMechanisms = da.GetValues(typeof(string)).Select(s => s.ToString()).ToList();
                rdse.SupportedSASMechanisms = supportedSASLMechanisms;
            } },
            { "namingcontexts", (da, rdse) =>
            {
                var namingContexts = da.GetValues(typeof(string)).Select(s => s.ToString()).ToList();
                rdse.NamingContexts = namingContexts;
            } },
            { "supportedcapabilities", (da, rdse) =>
            {
                var supportedCapabilities = da.GetValues(typeof(string)).Select(s => s.ToString()).ToList();
                rdse.SupportedCapabilities = supportedCapabilities;
            } },
            { "defaultnamingcontext", (da, rdse) =>
            {
                var defaultNamingContext = da.GetValues(typeof(string)).First().ToString();
                rdse.DefaultNamingContext = defaultNamingContext;
            } },
            { "schemanamingcontext", (da, rdse) =>
            {
                var schemaNamingContext = da.GetValues(typeof(string)).First().ToString();
                rdse.SchemaNamingContext = schemaNamingContext;
            } },
            { "subschemasubentry", (da, rdse) =>
            {
                var subSchemaSubEntry = da.GetValues(typeof(string)).First().ToString();
                rdse.SubSchemaSubEntry = subSchemaSubEntry;
            } },
            { "configurationnamingcontext", (da, rdse) =>
            {
                var configurationNamingContext = da.GetValues(typeof(string)).First().ToString();
                rdse.ConfigurationNamingContext = configurationNamingContext;
            } },
            { "servername", (da, rdse) =>
            {
                var servername = da.GetValues(typeof(string)).First().ToString();
                rdse.ServerName = servername;
            } }
        };
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;
        private RootDSE _rootDSE;

        public LdapHelper(IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public RootDSE GetRootDSE()
        {
            using (var connection = AuthenticateAsAdmin())
            {
                var searchRequest = new SearchRequest(null, "(objectClass=*)", SearchScope.Base, null);
                var response = connection.SendRequest(searchRequest);
                if (response.ResultCode != ResultCode.Success)
                {
                    return null;
                }

                var searchResponse = response as SearchResponse;
                if (searchResponse.Entries.Count != 1)
                {
                    return null;
                }

                var result = new RootDSE();
                var firstEntry = searchResponse.Entries[0];
                foreach (DictionaryEntry attribute in firstEntry.Attributes)
                {
                    var key = attribute.Key.ToString().ToLowerInvariant();
                    if (!_mapping.ContainsKey(key))
                    {
                        continue;
                    }

                    var directoryAttribute = attribute.Value as DirectoryAttribute;
                    _mapping[key](directoryAttribute, result);
                }

                return result;
            }
        }

        public LdapConnection AuthenticateAsAdmin()
        {
            LdapConnection result;
            var ldapDirectoryIdentifier = GetLdapDirectoryIdentifier();
            var networkCredential = GetAdminNetworkCredential();
            try
            {
                result = new LdapConnection(ldapDirectoryIdentifier);
                result.AuthType = _identityStoreLDAPOptions.Authenticate.AuthType;
                result.SessionOptions.ProtocolVersion = _identityStoreLDAPOptions.Authenticate.Version;
                result.SessionOptions.Sealing = true;
                result.SessionOptions.Signing = true;
                if (networkCredential != null)
                {
                    result.Bind(networkCredential);
                }
                else
                {
                    result.Bind();
                }
                
                return result;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public SearchResultEntryCollection GetCollection(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var rootDSE = GetCachedRootDSE();
            using (var connection = AuthenticateAsAdmin())
            {
                var searchRequest = new SearchRequest(rootDSE.DefaultNamingContext, filter, SearchScope.Subtree, null);
                var searchResponse = connection.SendRequest(searchRequest);
                if (searchResponse.ResultCode != ResultCode.Success)
                {
                    return null;
                }

                var result = searchResponse as SearchResponse;
                return result.Entries;
            }
        }

        public SearchResultEntryCollection GetCredentialSettings(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var filters = new List<string>();
            foreach (var id in ids)
            {
                filters.Add($"({_identityStoreLDAPOptions.Server.LDAPCredentialSettingAttributeIdentifier}={id})");
            }

            var filter = $"(|{string.Join("", filters)})";
            return GetCollection(filter);
        }

        public SearchResultEntry Get(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var rootDSE = GetCachedRootDSE();
            using (var connection = AuthenticateAsAdmin())
            {
                var searchRequest = new SearchRequest(rootDSE.DefaultNamingContext, filter, SearchScope.Subtree, null);
                var searchResponse = connection.SendRequest(searchRequest);
                if (searchResponse.ResultCode != ResultCode.Success)
                {
                    return null;
                }

                var result = searchResponse as SearchResponse;
                if (result.Entries.Count != 1)
                {
                    return null;
                }

                return result.Entries[0];
            }
        }

        public LdapConnection AuthenticateBasicLoginAndPassword(string login, string password, string domain = null)
        {
            LdapConnection result;
            var ldapDirectoryIdentifier = GetLdapDirectoryIdentifier();
            var networkCredential = new NetworkCredential(login, password, domain);
            try
            {
                result = new LdapConnection(ldapDirectoryIdentifier);
                result.AuthType = AuthType.Basic;
                result.SessionOptions.ProtocolVersion = 3;
                result.Bind(networkCredential);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public SearchResultEntryCollection GetAllUsers()
        {
            return GetCollection(_identityStoreLDAPOptions.Server.LDAPFilterAllUsers);
        }

        public SearchResultEntryCollection GetAllCredentialSettings()
        {
            return GetCollection(_identityStoreLDAPOptions.Server.LDAPFilterAllCredentialSettings);
        }

        public SearchResultEntryCollection GetPagedUsers(int pageSize, int startIndex)
        {
            var rootDSE = GetCachedRootDSE();
            using (var connection = AuthenticateAsAdmin())
            {
                var searchRequest = new SearchRequest(rootDSE.DefaultNamingContext, _identityStoreLDAPOptions.Server.LDAPFilterAllUsers,
                    SearchScope.Subtree, null);
                var prc = new PageResultRequestControl(pageSize);
                searchRequest.Controls.Add(prc);
                int currentIndex = 0;
                while (true)
                {
                    var ldapResponse = connection.SendRequest(searchRequest);
                    if (ldapResponse.ResultCode != ResultCode.Success)
                    {
                        return null;
                    }

                    var searchResponse = ldapResponse as SearchResponse;
                    if (currentIndex == startIndex)
                    {
                        return searchResponse.Entries;
                    }

                    currentIndex += pageSize;
                    foreach (var control in searchResponse.Controls)
                    {
                        if (control is PageResultResponseControl)
                        {
                            prc.Cookie = ((PageResultResponseControl)control).Cookie;
                            break;
                        }
                    }
                }
            }
        }

        public SearchResultEntryCollection GetUsers(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var filters = new List<string>();
            foreach(var id in ids)
            {
                filters.Add($"({_identityStoreLDAPOptions.Server.LDAPUserAttributeIdentifier}={id})");
            }

            var filter = $"(|{string.Join("", filters)})";
            return GetCollection(filter);
        }

        public SearchResultEntry GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Get($"({_identityStoreLDAPOptions.Server.LDAPUserAttributeIdentifier}={id})");
        }

        public SearchResultEntry GetUserCredential(string userId, string type)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Get($"({_identityStoreLDAPOptions.Server.LDAPUserCredentialAttributeIdentifier}={userId}_{type})");
        }

        public SearchResultEntry GetCredentialSetting(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Get($"({_identityStoreLDAPOptions.Server.LDAPCredentialSettingAttributeIdentifier}={id})");
        }

        public bool DeleteUser(string userId)
        {
            return Delete($"({_identityStoreLDAPOptions.Server.LDAPUserAttributeIdentifier}={userId})");
        }

        public bool DeleteUserCredential(string userId, string type)
        {
            return Delete($"({_identityStoreLDAPOptions.Server.LDAPUserCredentialAttributeIdentifier}={userId}_{type})");
        }

        public bool Delete(string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                throw new ArgumentNullException(nameof(distinguishedName));
            }

            using (var connection = AuthenticateAsAdmin())
            {
                var deleteRequest = new DeleteRequest(distinguishedName);
                var deleteResponse = connection.SendRequest(deleteRequest);
                if (deleteResponse.ResultCode != ResultCode.Success)
                {
                    return false;
                }

                return true;
            }
        }

        public bool InsertUserCredential(string userId, string type, ICollection<DirectoryAttribute> directoryAttributes)
        {
            return Insert($"({_identityStoreLDAPOptions.Server.LDAPUserCredentialAttributeIdentifier}={userId}_{type})", directoryAttributes);
        }

        public bool InsertUser(string userId, ICollection<DirectoryAttribute> directoryAttributes)
        {
            return Insert($"({_identityStoreLDAPOptions.Server.LDAPUserAttributeIdentifier}={userId})", directoryAttributes);
        }

        public bool Insert(string distinguishedName, ICollection<DirectoryAttribute> directoryAttributes)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                throw new ArgumentNullException(nameof(distinguishedName));
            }

            if (directoryAttributes == null)
            {
                throw new ArgumentNullException(nameof(directoryAttributes));
            }

            using (var connection = AuthenticateAsAdmin())
            {
                var insertRequest = new AddRequest(distinguishedName, directoryAttributes.ToArray());
                var ldapResponse = connection.SendRequest(insertRequest);
                return ldapResponse.ResultCode == ResultCode.Success;
            }
        }

        public bool UpdateUser(string userId, ICollection<DirectoryAttributeModification> directoryAttributeModifications)
        {
            return Update($"({_identityStoreLDAPOptions.Server.LDAPUserAttributeIdentifier}={userId})", directoryAttributeModifications);
        }

        public bool UpdateUserCredential(string userId, string type, ICollection<DirectoryAttributeModification> directoryAttributeModifications)
        {
            return Update($"({_identityStoreLDAPOptions.Server.LDAPUserCredentialAttributeIdentifier}={userId}_{type})", directoryAttributeModifications);
        }

        public bool UpdateCredentialSetting(string credentialSettingId, ICollection<DirectoryAttributeModification> directoryAttributeModifications)
        {
            return Update($"({_identityStoreLDAPOptions.Server.LDAPCredentialSettingAttributeIdentifier}={credentialSettingId})", directoryAttributeModifications);
        }

        public bool Update(string distinguishedName, ICollection<DirectoryAttributeModification> directoryAttributeModifications)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                throw new ArgumentNullException(nameof(distinguishedName));
            }

            if (directoryAttributeModifications == null)
            {
                throw new ArgumentNullException(nameof(directoryAttributeModifications));
            }

            using (var connection = AuthenticateAsAdmin())
            {
                var updateRequest = new ModifyRequest(distinguishedName, directoryAttributeModifications.ToArray());
                var ldapResponse = connection.SendRequest(updateRequest);
                return ldapResponse.ResultCode == ResultCode.Success;
            }
        }

        private NetworkCredential GetAdminNetworkCredential()
        {
            if (_identityStoreLDAPOptions.Authenticate == null)
            {
                return null;
            }

            return new NetworkCredential(_identityStoreLDAPOptions.Authenticate.UserName, _identityStoreLDAPOptions.Authenticate.Password, 
                _identityStoreLDAPOptions.Authenticate.Domain);
        }

        private LdapDirectoryIdentifier GetLdapDirectoryIdentifier()
        {
            return new LdapDirectoryIdentifier(_identityStoreLDAPOptions.Server.Server, _identityStoreLDAPOptions.Server.Port);
        }

        private RootDSE GetCachedRootDSE()
        {
            if (_rootDSE == null)
            {
                _rootDSE = GetRootDSE();
            }

            return _rootDSE;
        }
    }
}
