using System.Collections.Generic;

namespace SimpleIdServer.Uma.EF.Models
{
    public class ResourceSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Type { get; set; }
        public string IconUri { get; set; }
        public string Owner { get; set; }
        public virtual ICollection<ResourceScope> Scopes { get; set; }
        public virtual ICollection<ResourceSetPolicy> ResourceSetPolicies { get; set; }
    }
}
