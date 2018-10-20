using System.Runtime.Serialization;

namespace SimpleIdServer.UserManagement.Common.Responses
{
    [DataContract]
    public class AuthproviderResponse
    {
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        [DataMember(Name = "authentication_scheme")]
        public string AuthenticationScheme { get; set; }
    }
}
