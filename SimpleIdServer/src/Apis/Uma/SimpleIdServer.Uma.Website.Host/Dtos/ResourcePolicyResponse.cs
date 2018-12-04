using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class ResourcePolicyResponse
    {
        [DataMember(Name = "policy_id")]
        public string PolicyId { get; set; }
        [DataMember(Name = "scopes")]
        public IEnumerable<string> Scopes { get; set; }
        [DataMember(Name = "permissions")]
        public IEnumerable<string> Permissions { get; set; }
    }
}
