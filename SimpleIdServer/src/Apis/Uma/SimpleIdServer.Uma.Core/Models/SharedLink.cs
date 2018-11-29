using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class SharedLink
    {
        public string ConfirmationCode { get; set; }
        public string ResourceId { get; set; }
        public IEnumerable<string> Scopes { get; set; } 
    }
}
