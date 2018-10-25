using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Responses
{
    [DataContract]
    public class GrantedTokenResponse
    {
        [DataMember(Name = Constants.GrantedTokenNames.AccessToken)]
        public string AccessToken { get; set; }
        [DataMember(Name = Constants.GrantedTokenNames.IdToken)]
        public string IdToken { get; set; }
        [DataMember(Name = Constants.GrantedTokenNames.TokenType)]
        public string TokenType { get; set; }
        [DataMember(Name = Constants.GrantedTokenNames.ExpiresIn)]
        public int ExpiresIn { get; set; }
        [DataMember(Name = Constants.GrantedTokenNames.RefreshToken)]
        public string RefreshToken { get; set; }
        [DataMember(Name = Constants.GrantedTokenNames.Scope)]
        public IEnumerable<string> Scope { get; set; }
    }
}
