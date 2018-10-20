using System.Collections.Generic;

namespace SimpleIdServer.Scim.Core.Results
{
    public class PaginatedResult<T>
    {
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public IEnumerable<T> Content { get; set; }
    }
}