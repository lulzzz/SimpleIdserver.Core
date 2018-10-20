using SimpleIdServer.AccountFilter.Basic.Common.Responses;
using SimpleIdServer.Common.Client;

namespace SimpleIdServer.AccountFilter.Basic.Client.Results
{
    public class GetFilterResult : BaseResponse
    {
        public FilterResponse Content { get; set; }
    }
}
