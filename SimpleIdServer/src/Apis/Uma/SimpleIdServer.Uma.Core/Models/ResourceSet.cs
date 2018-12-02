using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class ResourceSet
    {        
        public string Id { get; set; }    
        public string Name { get; set; }        
        public string Uri { get; set; }        
        public string Type { get; set; }        
        public string IconUri { get; set; }
        public string Owner { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public IEnumerable<string> AuthorizationPolicyIds { get; set; }
        public IEnumerable<Policy> AuthPolicies { get; set; }
    }
}