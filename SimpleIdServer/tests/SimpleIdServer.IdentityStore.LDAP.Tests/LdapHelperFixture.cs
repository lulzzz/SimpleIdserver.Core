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
        public void When_Authenticate_With_Admin_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var ldapConnection = _ldapHelper.AuthenticateAsAdmin();

            // ASSERT
            Assert.NotNull(ldapConnection);
            ldapConnection.Dispose();
        }

        [Fact]
        public void When_Authenticate_With_LoginPassword_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var ldapConnection = _ldapHelper.AuthenticateBasicLoginAndPassword("cn=thabart,cn=users,cn=system", "password");

            // ASSERT
            Assert.NotNull(ldapConnection);
            ldapConnection.Dispose();
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
        public void When_GetAllCredentialSettings_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetAllCredentialSettings();

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
                "thabart",
                "laetitia"
            });

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void When_GetPagedResult_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var firstUser = _ldapHelper.GetPagedUsers(1, 0);
            var secondUser = _ldapHelper.GetPagedUsers(1, 1);

            // ASSERT
            Assert.NotNull(firstUser);
            Assert.NotNull(secondUser);
            Assert.Equal(1, firstUser.Count);
            Assert.Equal(1, secondUser.Count);
        }

        [Fact]
        public void When_GetUser_Then_One_Record_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _ldapHelper.GetUser("thabart");

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void When_GetUserCredential_Then_One_Record_Is_Returned()
        {
            // TODO
        }

        [Fact]
        public void When_GetCredentialSetting_Then_One_Record_Is_Returned()
        {
            // TODO
        }

        [Fact]
        public void When_DeleteUser_Then_Ok_Is_Returned()
        {
            // TODO
        }

        [Fact]
        public void When_DeleteUserCredential_Then_Ok_Is_Returned()
        {
            // TODO
        }

        private void InitializeFakeObjects()
        {
            _options = new IdentityStoreLDAPOptions
            {
                Server = new LDAPServerOptions
                {
                    Server = "127.0.0.1",
                    Port = 389,
                    // Port = 10389,
                    LDAPFilterAllUsers = "(objectClass=person)",
                    LDAPFilterAllCredentialSettings = "(objectClass=credentialSetting)",
                    LDAPUserAttributeIdentifier = "cn"
                },
                Authenticate = new LDAPAuthenticateOptions
                {
                    UserName = "cn=thabart,cn=users,cn=system",
                    // UserName = "uid=admin,ou=system",
                    Password = "password",
                    AuthType = System.DirectoryServices.Protocols.AuthType.Basic,
                    Version = 3
                }
            };
            _ldapHelper = new LdapHelper(_options);
        }
    }
}
