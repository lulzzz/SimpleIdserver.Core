using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.IdentityStore.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Domains = SimpleIdServer.IdentityStore.Models;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;

        public UserRepository(IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public Task<bool> Authenticate(string login, string password)
        {
            using (var ldapHelper = new LdapHelper())
            {
                return Task.FromResult(ldapHelper.Connect(_identityStoreLDAPOptions.Server, _identityStoreLDAPOptions.Port, login, password));
            }
        }

        public Task<bool> DeleteAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public Task<Domains.User> Get(string id)
        {
            using (var ldapHelper = new LdapHelper())
            {
                ldapHelper.Connect(_identityStoreLDAPOptions.Server, _identityStoreLDAPOptions.Port, _identityStoreLDAPOptions.UserName, _identityStoreLDAPOptions.Password);
                var search = ldapHelper.Search(_identityStoreLDAPOptions.LDAPBaseDN, string.Format(_identityStoreLDAPOptions.LDAPFilterUser, id));
                if (search.Entries.Count != 1)
                {
                    return Task.FromResult((Domains.User)null);
                }

                var firstEntry = search.Entries[0];
                var result = new Domains.User
                {
                    Id = firstEntry.DistinguishedName,
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "thabart"),
                        new Claim("name", "Thierry Habart")
                    },
                    IsBlocked = false,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow,
                    Credentials = new List<Domains.UserCredential>
                    {
                        new Domains.UserCredential
                        {
                            Type = "pwd",
                            IsBlocked = false,
                            ExpirationDateTime = DateTime.UtcNow.AddYears(10)
                        }
                    }
                };
                return Task.FromResult(result);
            }
        }

        public Task<ICollection<Domains.User>> Get(IEnumerable<Claim> claims)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Domains.User>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Domains.User> GetUserByClaim(string key, string value)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertAsync(Domains.User user)
        {
            throw new NotImplementedException();
        }

        public Task<SearchUserResult> Search(SearchUserParameter parameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Domains.User user)
        {
            throw new NotImplementedException();
        }
    }
}
