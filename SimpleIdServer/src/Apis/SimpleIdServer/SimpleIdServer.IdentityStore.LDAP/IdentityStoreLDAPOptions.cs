namespace SimpleIdServer.IdentityStore.LDAP
{
    public class IdentityStoreLDAPOptions
    {
        // LDAP CONFIGURATION
        public string Server { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LDAPBaseDN { get; set; }
        public string LDAPFilterUser { get; set; }
    }
}