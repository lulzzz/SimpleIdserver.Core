using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class ShareResourceRequest
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "scopes")]
        public IEnumerable<string> Scopes { get; set; }
    }
}