using System;
using System.Collections.Generic;
using SimpleIdServer.Dtos.Responses;

namespace SimpleIdServer.AccessToken.Store
{
    public class StoredToken
    {
        public StoredToken()
        {
            Scopes = new List<string>();
        }

        public string Url { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public GrantedTokenResponse GrantedToken { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
