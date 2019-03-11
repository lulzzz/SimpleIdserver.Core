using System;
using System.Collections.Generic;

namespace SimpleIdServer.IdentityStore.EF.Models
{
    public class Claim
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public bool IsIdentifier { get; set; }
        public virtual ICollection<UserClaim> UserClaims { get; set; }
    }
}
