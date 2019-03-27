using SimpleIdServer.IdentityStore.LDAP.Extensions;
using SimpleIdServer.IdentityStore.Parameters;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.IdentityStore.Results;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domains = SimpleIdServer.IdentityStore.Models;

namespace SimpleIdServer.IdentityStore.LDAP.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly ILdapHelperFactory _ldapHelperFactory;
        private readonly IdentityStoreLDAPOptions _identityStoreLDAPOptions;

        public UserRepository(ILdapHelperFactory ldapHelperFactory, IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            _ldapHelperFactory = ldapHelperFactory;
            _identityStoreLDAPOptions = identityStoreLDAPOptions;
        }

        public Task<bool> Authenticate(string login, string password)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            return Task.FromResult(ldapHelper.AuthenticateBasicLoginAndPassword(login, password) != null);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            return Task.FromResult(ldapHelper.DeleteUser(id));
        }

        public Task<Domains.User> Get(string id)
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntry = ldapHelper.GetUser(id);
            if (searchResultEntry == null)
            {
                return Task.FromResult((Domains.User)null);
            }

            return Task.FromResult(GetUser(searchResultEntry));
        }

        public Task<ICollection<Domains.User>> Get(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var filters = new List<string>();
            foreach(var claim in claims)
            {
                filters.Add(BuildFilter(claim.Type, claim.Value));
            }

            var filter = $"(|{string.Join("", filters.ToArray())})";
            var ldapHelper = _ldapHelperFactory.Build();
            var users = ldapHelper.GetCollection(filter);
            ICollection<Domains.User> result = new List<Domains.User>();
            foreach(SearchResultEntry searchResultEntry in users)
            {
                result.Add(GetUser(searchResultEntry));
            }

            return Task.FromResult(result);
        }

        public Task<ICollection<Domains.User>> GetAll()
        {
            var ldapHelper = _ldapHelperFactory.Build();
            var searchResultEntryCollection = ldapHelper.GetAllUsers();
            ICollection<Domains.User> result = new List<Domains.User>();
            foreach(SearchResultEntry record in searchResultEntryCollection)
            {
                result.Add(GetUser(record));
            }

            return Task.FromResult(result);            
        }

        public Task<Domains.User> GetUserByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var ldapAttributeName = GetLDAPAttributeName(key);
            if (ldapAttributeName == null)
            {
                return Task.FromResult((Domains.User)null);
            }

            var ldapHelper = _ldapHelperFactory.Build();     
            var searchResultEntry = ldapHelper.Get(BuildFilter(ldapAttributeName, value));
            if (searchResultEntry == null)
            {
                return Task.FromResult((Domains.User)null);
            }

            return Task.FromResult(GetUser(searchResultEntry));            
        }

        public Task<bool> InsertAsync(Domains.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var directoryAttributes = new List<DirectoryAttribute>();
            if(user.Claims != null)
            {
                foreach(var cl in user.Claims)
                {
                    var ldapAttr = GetLDAPAttributeName(cl.Type);
                    if (string.IsNullOrWhiteSpace(ldapAttr))
                    {
                        continue;
                    }

                    directoryAttributes.Add(new DirectoryAttribute(ldapAttr, cl.Value));
                }
            }

            var ldapHelper = _ldapHelperFactory.Build();
            return Task.FromResult(ldapHelper.InsertUser(user.Id, directoryAttributes));
        }

        public Task<SearchUserResult> Search(SearchUserParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var ldapHelper = _ldapHelperFactory.Build();            
            SearchResultEntryCollection searchResultEntryCollection;
            if (parameter.Subjects == null || !parameter.Subjects.Any())
            {
                searchResultEntryCollection = ldapHelper.GetPagedUsers(parameter.Count, parameter.StartIndex);
            }
            else
            {
                searchResultEntryCollection = ldapHelper.GetUsers(parameter.Subjects);
            }

            ICollection<Domains.User> users = new List<Domains.User>();
            foreach(SearchResultEntry record in searchResultEntryCollection)
            {
                users.Add(GetUser(record));
            }

            var result = new SearchUserResult
            {
                Content = users,
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.Count
            };
            return Task.FromResult(result);            
        }

        public Task<bool> UpdateAsync(Domains.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            var distinguishedName = user.Id;
            var directoryAttributes = new List<DirectoryAttributeModification>();
            if (user.Claims != null)
            {
                foreach (var cl in user.Claims)
                {
                    var ldapAttr = GetLDAPAttributeName(cl.Type);
                    if (string.IsNullOrWhiteSpace(ldapAttr))
                    {
                        continue;
                    }

                    var directoryAttributeModification = new DirectoryAttributeModification
                    {
                        Name = ldapAttr,
                        Operation = DirectoryAttributeOperation.Replace
                    };
                    directoryAttributeModification.Add(cl.Value);
                    directoryAttributes.Add(directoryAttributeModification);
                }
            }

            var ldapHelper = _ldapHelperFactory.Build();
            var result = ldapHelper.UpdateUser(distinguishedName, directoryAttributes);
            return Task.FromResult(result);
        }

        private string GetLDAPAttributeName(string openidClaimName)
        {
            var claimMapping = _identityStoreLDAPOptions.User.ClaimMappings.FirstOrDefault(c => c.OpenidClaim == openidClaimName);
            if (claimMapping == null)
            {
                return null;
            }

            return claimMapping.LDAPAttribute;
        }
        
        private Domains.User GetUser(SearchResultEntry searchResultEntry)
        {
            var result = new Domains.User();
            var claims = new List<Claim>();
            foreach(var cm in _identityStoreLDAPOptions.User.ClaimMappings)
            {
                var attrs = searchResultEntry.Attributes.GetAttributes(cm.LDAPAttribute);
                if (!attrs.Any())
                {
                    continue;
                }

                foreach(var attr in attrs)
                {
                    claims.Add(new Claim(cm.OpenidClaim, attr));
                }
            }

            result.Id = searchResultEntry.DistinguishedName;
            result.Claims = claims;
            return result;
        }

        private static string BuildFilter(string key, string value)
        {
            return $"({key}={value})";
        }
    }
}