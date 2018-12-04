using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class UpdateResourcePermissionsRequest
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "policy_ids")]
        public IEnumerable<string> PolicyIds { get; set; }
    }
}
