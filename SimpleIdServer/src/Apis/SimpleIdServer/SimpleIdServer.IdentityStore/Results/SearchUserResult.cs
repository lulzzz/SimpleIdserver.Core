using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;

namespace SimpleIdServer.IdentityStore.Results
{
    public class SearchUserResult
    {
        public IEnumerable<User> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}