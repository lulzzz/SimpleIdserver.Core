using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Common.DTOs
{
    [DataContract]
    public class PolicyResponse
    {
        [DataMember(Name = PolicyNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = PolicyNames.ResourceSetIds)]
        public IEnumerable<string> ResourceSetIds { get; set; }
        [DataMember(Name = PolicyRuleNames.Scopes)]
        public IEnumerable<string> Scopes { get; set; }
        [DataMember(Name = PolicyRuleNames.Claims)]
        public IEnumerable<PostClaim> Claims { get; set; }
        [DataMember(Name = PolicyRuleNames.IsResourceOwnerConsentNeeded)]
        public bool IsResourceOwnerConsentNeeded { get; set; }
        [DataMember(Name = PolicyRuleNames.Script)]
        public string Script { get; set; }
    }
}
