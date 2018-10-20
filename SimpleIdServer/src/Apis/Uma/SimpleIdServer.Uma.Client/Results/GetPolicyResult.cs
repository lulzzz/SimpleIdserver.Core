using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class GetPolicyResult : BaseResponse
    {
        public PolicyResponse Content { get; set; }
    }
}
