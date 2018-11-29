using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class UpdateResourcePermissionRequest
    {
        [DataMember(Name = "policy_id")]
        public string PolicyId { get; set; }
        [DataMember(Name = "rules")]
        public IEnumerable<string> Rules { get; set; }
    }
}
