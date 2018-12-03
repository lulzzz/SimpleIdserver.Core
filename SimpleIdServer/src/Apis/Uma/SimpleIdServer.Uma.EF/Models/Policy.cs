using System.Collections.Generic;

namespace SimpleIdServer.Uma.EF.Models
{
    public class Policy
    {
        public string Id { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public string Script { get; set; }
        public virtual ICollection<PolicyScope> Scopes { get; set; }
        public virtual ICollection<PolicyClaim> Claims { get; set; }
        public virtual ICollection<ResourceSetPolicy> ResourceSetPolicies{ get; set; }
        public virtual ICollection<PolicyClient> Clients { get; set; }
    }
}
