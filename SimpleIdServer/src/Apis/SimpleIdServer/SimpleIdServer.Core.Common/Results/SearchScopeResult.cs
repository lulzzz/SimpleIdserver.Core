using System.Collections.Generic;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.Core.Common.Results
{
    public class SearchScopeResult
    {
        public IEnumerable<Scope> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
