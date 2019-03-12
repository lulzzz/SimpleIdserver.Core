using System.Collections.Generic;

namespace SimpleIdServer.EF.Models
{
    public class Consent
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string UserId { get; set; } 
        public Client Client { get; set; }
        public virtual ICollection<ConsentScope> ConsentScopes { get; set; }
        public virtual ICollection<ConsentClaim> ConsentClaims { get; set; }
    }
}
