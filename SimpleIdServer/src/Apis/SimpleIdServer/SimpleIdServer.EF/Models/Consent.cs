using System.Collections.Generic;

namespace SimpleIdServer.EF.Models
{
    public class Consent
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public string ResourceOwnerId { get; set; } 

        public Client Client { get; set; }

        public ResourceOwner ResourceOwner { get; set; }

        public virtual List<ConsentScope> ConsentScopes { get; set; }

        public virtual List<ConsentClaim> ConsentClaims { get; set; }
    }
}
