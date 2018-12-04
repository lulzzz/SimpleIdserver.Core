using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class SearchPendingRequestResult
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public IEnumerable<PendingRequest> PendingRequests { get; set; }
    }
}
