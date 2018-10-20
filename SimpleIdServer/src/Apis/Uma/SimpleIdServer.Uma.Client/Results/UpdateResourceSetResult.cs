using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class UpdateResourceSetResult : BaseResponse
    {
        public UpdateResourceSetResponse Content { get; set; }
    }
}
