using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class DatatableColumnRequest
    {
        [DataMember(Name = "data")]
        public int Data { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
