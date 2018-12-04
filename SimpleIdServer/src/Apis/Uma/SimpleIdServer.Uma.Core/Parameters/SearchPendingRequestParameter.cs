using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class SearchPendingRequestParameter
    {
        public SearchPendingRequestParameter()
        {
            IsPagingEnabled = true;
        }

        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public IEnumerable<string> Owners { get; set; }
        public bool? IsConfirmed { get; set; }
    }
}
