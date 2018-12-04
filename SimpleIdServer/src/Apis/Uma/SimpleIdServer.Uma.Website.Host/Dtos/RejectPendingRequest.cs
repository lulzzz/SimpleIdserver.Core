using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class RejectPendingRequest
    {
        [DataMember(Name = "pending_request_id")]
        public string PendingRequestId { get; set; }
    }
}
