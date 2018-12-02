using SimpleIdServer.Uma.Core.Models;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Core.Policies
{
    [DataContract]
    public enum AuthorizationPolicyResultEnum
    {
        [EnumMember(Value = "not_authorized")]
        NotAuthorized,
        [EnumMember(Value = "need_info")]
        NeedInfo,
        [EnumMember(Value = "request_submitted")]
        RequestSubmitted,
        [EnumMember(Value = "request_not_confirmed")]
        RequestNotConfirmed,
        [EnumMember(Value = "authorized")]
        Authorized
    }

    public class AuthorizationPolicyResult
    {
        public AuthorizationPolicyResultEnum Type { get; set; }
        public object ErrorDetails { get; set; }
        public string Subject { get; set; }
        public Policy Policy { get; set; }
    }
}
