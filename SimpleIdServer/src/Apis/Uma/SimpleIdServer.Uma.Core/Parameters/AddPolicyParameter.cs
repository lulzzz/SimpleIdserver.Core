using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class AddClaimParameter
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class AddPolicyParameter
    {
        public List<string> ResourceSetIds { get; set; }
        public List<string> Scopes { get; set; }
        public List<string> ClientIdsAllowed { get; set; }
        public List<AddClaimParameter> Claims { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public string Script { get; set; }
    }
}
