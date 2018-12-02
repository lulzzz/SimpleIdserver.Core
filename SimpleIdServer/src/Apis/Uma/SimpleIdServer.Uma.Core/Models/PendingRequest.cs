using System;

namespace SimpleIdServer.Uma.Core.Models
{
    public class PendingRequest
    {
        public string AuthorizationPolicyRuleId { get; set; }
        public string RequesterSubject { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}