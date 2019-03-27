using Xunit;

namespace SimpleIdServer.IdentityStore.LDAP.Tests
{
    public class LdapHelperFixture
    {
        private IdentityStoreLDAPOptions _options;
        private ILdapHelper _ldapHelper;

        [Fact]
        public void When_GetRootDSE_Then_Result_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var rootDSE = _ldapHelper.GetRootDSE();

            // ASSERT
            Assert.NotNull(rootDSE);
        }

        [Fact]
        public void When_GetPagedResult_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetPagedUsers(10, 0);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public void When_GetAllUsers_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetAllUsers();

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void When_GetUser_Then_One_Record_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetUser("Habart Thierry");

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void When_GetMultipleUsers_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetUsers(new[]
            {
                "Habart Thierry"
            });

            // ASSERT
            Assert.NotNull(result);
        }
        
        private void InitializeFakeObjects()
        {
            _options = new IdentityStoreLDAPOptions
            {
                Server = new LDAPServerOptions
                {
                    Server = "127.0.0.1",
                    Port = 389,
                    LDAPFilterAllUsers = "(objectClass=person)",
                    LDAPUserAttributeIdentifier = "cn"
                },
                Authenticate = new LDAPAuthenticateOptions
                {
                    UserName = "cn=thabart,cn=users,cn=system",
                    Password = "password",
                    AuthType = System.DirectoryServices.Protocols.AuthType.Basic,
                    Version = 3
                }
            };
            _ldapHelper = new LdapHelper(_options);
        }
    }
}
