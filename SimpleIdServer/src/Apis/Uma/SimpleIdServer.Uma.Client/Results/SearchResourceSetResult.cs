using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
