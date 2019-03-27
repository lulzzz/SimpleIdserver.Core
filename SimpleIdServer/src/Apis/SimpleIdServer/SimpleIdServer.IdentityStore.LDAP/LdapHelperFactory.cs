namespace SimpleIdServer.IdentityStore.LDAP
{
    public interface ILdapHelperFactory
    {
        ILdapHelper Build();
    }

    internal sealed class LdapHelperFactory : ILdapHelperFactory
    {
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;

        public LdapHelperFactory(IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public ILdapHelper Build()
        {
            return new LdapHelper(_identityStoreLDAPOptions);
        }
    }
}
