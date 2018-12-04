using System;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.EF.Models
{
    public class ResourcePendingRequest
    {
        public string Id { get; set; }
        public string ResourceId { get; set; }
        public string RequesterSubject { get; set; }
        public DateTime CreateDateTime { get; set; }
        public bool IsConfirmed { get; set; }
        public virtual ResourceSet Resource { get; set; }
        public virtual ICollection<PendingRequestScope> Scopes { get; set; }
    }
}
