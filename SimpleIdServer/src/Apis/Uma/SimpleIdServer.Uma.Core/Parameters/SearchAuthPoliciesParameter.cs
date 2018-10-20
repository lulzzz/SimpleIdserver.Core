using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class SearchAuthPoliciesParameter
    {
        public SearchAuthPoliciesParameter()
        {
            IsPagingEnabled = true;
        }

        public IEnumerable<string> Ids { get; set; }
        public IEnumerable<string> ResourceIds { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public bool IsPagingEnabled { get; set; }
    }
}
