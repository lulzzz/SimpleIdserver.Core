using SimpleIdServer.Common.Client;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Client.Results
{
    public class GetResourcesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}