using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class ShareResourceResponse
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
