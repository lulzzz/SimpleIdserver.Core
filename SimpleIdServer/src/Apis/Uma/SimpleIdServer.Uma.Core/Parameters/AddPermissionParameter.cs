using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class AddPermissionParameter
    {
        public string ResourceSetId { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
