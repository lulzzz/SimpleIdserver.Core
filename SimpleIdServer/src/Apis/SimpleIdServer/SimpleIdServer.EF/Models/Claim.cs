using System;
using System.Collections.Generic;

namespace SimpleIdServer.EF.Models
{
    public class Claim
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public bool IsIdentifier { get; set; }
        public virtual List<ScopeClaim> ScopeClaims { get; set; }
        public virtual List<ConsentClaim> ConsentClaims { get; set; }
    }
}