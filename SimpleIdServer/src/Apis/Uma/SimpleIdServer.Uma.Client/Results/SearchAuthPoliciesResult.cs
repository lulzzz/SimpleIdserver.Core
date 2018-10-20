using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class SearchAuthPoliciesResult : BaseResponse
    {
        public SearchAuthPoliciesResponse Content { get; set; }
    }
}
