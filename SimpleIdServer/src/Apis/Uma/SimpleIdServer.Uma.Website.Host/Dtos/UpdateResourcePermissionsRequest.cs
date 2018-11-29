using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class UpdateResourcePermissionsRequest
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "policies")]
        public IEnumerable<UpdateResourcePermissionRequest> Policies { get; set; }
    }
}
