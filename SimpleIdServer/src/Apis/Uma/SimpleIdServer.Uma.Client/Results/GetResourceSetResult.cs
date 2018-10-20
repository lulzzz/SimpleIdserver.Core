using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class GetResourceSetResult : BaseResponse
    {
        public ResourceSetResponse Content { get; set; }
    }
}
