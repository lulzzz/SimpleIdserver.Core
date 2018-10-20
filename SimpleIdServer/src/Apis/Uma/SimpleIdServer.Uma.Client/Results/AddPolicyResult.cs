using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class AddPolicyResult : BaseResponse
    {
        public AddPolicyResponse Content { get; set; }
    }
}