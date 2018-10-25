using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Requests
{
    [DataContract]
    public class RevokeSessionRequest
    {
        [DataMember(Name = Constants.RevokeSessionRequestNames.IdTokenHint)]
        public string IdTokenHint { get; set; }
        [DataMember(Name = Constants.RevokeSessionRequestNames.PostLogoutRedirectUri)]
        public string PostLogoutRedirectUri { get; set; }
        [DataMember(Name = Constants.RevokeSessionRequestNames.State)]
        public string State { get; set; }
    }
}
