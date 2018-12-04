using SimpleIdServer.Uma.Core.Models;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Core.Policies
{
    [DataContract]
    public enum AuthorizationPolicyResultEnum
    {
        NotAuthorized,
        NeedInfo,
        RequestSubmitted,
        RequestNotConfirmed,
        Authorized
    }

    public class AuthorizationPolicyResult
    {
        public AuthorizationPolicyResultEnum Type { get; set; }
        public object ErrorDetails { get; set; }
        public Policy Policy { get; set; }
        public string Subject { get; set; }
    }
}