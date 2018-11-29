using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class ShareResourceParameter
    {
        public string Owner { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ResourceId { get; set; }
    }
}
