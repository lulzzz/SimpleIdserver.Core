using SimpleIdServer.AccountFilter.Basic.Common.Responses;
using SimpleIdServer.Common.Client;

namespace SimpleIdServer.AccountFilter.Basic.Client.Results
{
    public class AddFilterResult : BaseResponse
    {
        public AddFilterResponse Content { get; set; }
    }
}
