using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class UpdatePolicyParameter
    {
        public string PolicyId { get; set; }
        public List<string> ClientIdsAllowed { get; set; }
        public List<string> Scopes { get; set; }
        public List<AddClaimParameter> Claims { get; set; }
        public string Script { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
    }
}
