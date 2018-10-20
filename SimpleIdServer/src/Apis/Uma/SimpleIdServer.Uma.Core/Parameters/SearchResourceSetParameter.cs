using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class SearchResourceSetParameter
    {
        public SearchResourceSetParameter()
        {
            IsPagingEnabled = true;
        }

        public IEnumerable<string> Ids { get; set; }
        public IEnumerable<string> Names { get; set; }
        public IEnumerable<string> Types { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public bool IsPagingEnabled { get; set; }
    }
}
