using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class Claim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class Policy
    {
        public string Id { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public List<string> Scopes { get; set; }
        public List<Claim> Claims { get; set; }
        public IEnumerable<string> ResourceSetIds { get; set; }
        public string Script { get; set; }
    }
}
