using System.Runtime.Serialization;

namespace SimpleIdServer.UserManagement.Common.Requests
{
    [DataContract]
    public sealed class LinkProfileRequest
    {
        [DataMember(Name = Constants.LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = Constants.LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = Constants.LinkProfileRequestNames.Force)]
        public bool Force { get; set; }
    }
}
