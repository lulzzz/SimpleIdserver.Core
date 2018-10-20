using System.Collections.Generic;
using SimpleIdServer.Scim.Core.EF.Models;

namespace SimpleIdServer.Scim.Host
{
    public class ScimServerConfiguration
    {
        public List<Representation> Representations { get; set; }
        public List<Schema> Schemas { get; set; }
    }

    public class ScimServerOptions
    {
        public ScimServerConfiguration ServerConfiguration { get; set; }
    }
}
