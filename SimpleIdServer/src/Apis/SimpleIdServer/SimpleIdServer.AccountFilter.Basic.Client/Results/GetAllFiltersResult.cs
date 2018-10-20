using System.Collections.Generic;
using SimpleIdServer.AccountFilter.Basic.Common.Responses;
using SimpleIdServer.Common.Client;

namespace SimpleIdServer.AccountFilter.Basic.Client.Results
{
    public class GetAllFiltersResult : BaseResponse
    {
        public IEnumerable<FilterResponse> Content { get; set; }
    }
}
