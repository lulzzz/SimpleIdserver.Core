using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Parameters
{
    public class UpdateResourcePermissionParameter
    {
        public string PolicyId { get; set; }
        public IEnumerable<string> RuleIds { get; set; }
    }

    public class UpdateResourcePermissionsParameter
    {
        public string Subject { get; set; }
        public string ResourceId { get; set; }
        public IEnumerable<UpdateResourcePermissionParameter> Policies { get; set; }
    }
}
