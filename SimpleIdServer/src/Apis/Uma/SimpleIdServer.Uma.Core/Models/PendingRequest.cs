using System;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class PendingRequest
    {
        public string Id { get; set; }
        public string ResourceId { get; set; }
        public string RequesterSubject { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreateDateTime { get; set; }
        public ResourceSet Resource { get; set; }
    }
}