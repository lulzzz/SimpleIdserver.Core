using SimpleIdServer.Core.Common.Parameters;
using System.Collections.Generic;

namespace SimpleIdServer.IdentityStore.Parameters
{
    public class SearchUserParameter
    {
        public SearchUserParameter()
        {
            IsPagingEnabled = true;
        }

        public OrderParameter Order { get; set; }
        public IEnumerable<string> Subjects { get; set; }
        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}