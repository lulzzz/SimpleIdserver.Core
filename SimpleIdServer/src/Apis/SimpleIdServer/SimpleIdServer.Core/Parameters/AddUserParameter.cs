using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdServer.Core.Parameters
{
    public class AddUserParameter
    {
        public AddUserParameter(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }
        
        public string ExternalLogin { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
        public IEnumerable<AddUserCredentialParameter> Credentials { get; set; }
    }
}