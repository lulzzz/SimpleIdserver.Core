using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace SimpleIdServer.IdentityStore.LDAP
{
    public class LDAPAuthenticateOptions
    {
        public AuthType AuthType { get; set; }
        public int Version { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }

    public class LDAPServerOptions
    {
        public LDAPServerOptions()
        {
            Port = 389;
        }

        /// <summary>
        /// LDAP server.
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// LDAP Port.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// LDAP filter used to get all the users.
        /// </summary>
        public string LDAPFilterAllUsers { get; set; }
        /// <summary>
        /// LDAP filter used to get all the credential settings.
        /// </summary>
        public string LDAPFilterAllCredentialSettings { get; set; }
        /// <summary>
        /// LDAP attribute used as user identifier.
        /// </summary>
        public string LDAPUserAttributeIdentifier { get; set; }
        /// <summary>
        /// LDAP attribute used as credential setting identifier.
        /// </summary>
        public string LDAPCredentialSettingAttributeIdentifier { get; set; }
        /// <summary>
        /// LDAP attribute used as user credential identifier.
        /// </summary>
        public string LDAPUserCredentialAttributeIdentifier { get; set; }
    }

    public class LDAPClaimMapping
    {
        public string LDAPAttribute { get; set; }
        public string OpenidClaim { get; set; }
    }

    public class LDAPUserOptions
    {
        public LDAPUserOptions()
        {
            ClaimMappings = new List<LDAPClaimMapping>();
        }

        public string SubjectAttributeName { get; set; }
        public ICollection<LDAPClaimMapping> ClaimMappings { get; set; }
    }

    public class LDAPCredentialOptions
    {
        public string AuthenticationIntervalsInSecondsName { get; set; }
        public string CredentialTypeName { get; set; }
        public string ExpiresInName { get; set; }
        public string IsBlockAccountPolicyEnabledName { get; set; }
        public string NumberOfAuthenticationAttemptsName { get; set; }
        public string OptionsName { get; set; }
    }

    public class LDAPUserCredentialOptions
    {
        public string BlockedDateTimeName { get; set; }
        public string ExpirationDateTimeName { get; set; }
        public string FirstAuthenticationFailureDateTimeName { get; set; }
        public string IsBlockedName { get; set; }
        public string NumberOfAttemptsName { get; set; }
        public string TypeName { get; set; }
        public string UserIdName { get; set; }
        public string ValueName { get; set; }
    }

    public class IdentityStoreLDAPOptions
    {
        public IdentityStoreLDAPOptions()
        {
            Server = new LDAPServerOptions();
            Authenticate = new LDAPAuthenticateOptions();
            User = new LDAPUserOptions();
            Credential = new LDAPCredentialOptions();
            UserCredential = new LDAPUserCredentialOptions();
        }

        public LDAPServerOptions Server { get; set; }
        public LDAPAuthenticateOptions Authenticate { get; set; }
        public LDAPUserOptions User { get; set; }
        public LDAPCredentialOptions Credential { get; set; }
        public LDAPUserCredentialOptions UserCredential { get; set; }
    }
}