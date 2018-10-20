using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class AddResourceSetResult : BaseResponse
    {
        public AddResourceSetResponse Content { get; set; }
    }
}
