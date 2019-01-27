using System;

namespace SimpleIdServer.Core.Common.Models
{
    public class ResourceOwnerCredential
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime? FirstAuthenticationDateTime { get; set; }
        public int NumberOfAuthenticationAttempts { get; set; }
        public DateTime BlockedDateTime { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
