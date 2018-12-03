using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class UpdateResourcePermissionsParameter
    {
        public string Subject { get; set; }
        public string ResourceId { get; set; }
        public IEnumerable<string> PolicyIds{ get; set; }
    }
}
