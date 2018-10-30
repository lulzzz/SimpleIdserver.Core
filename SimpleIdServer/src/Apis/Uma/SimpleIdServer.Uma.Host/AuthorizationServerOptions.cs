using SimpleIdServer.Core;
using SimpleIdServer.Uma.Core.Models;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Host
{
    public class AuthorizationServerConfiguration
    {
        public List<ResourceSet> Resources { get; set; }
        public List<Policy> Policies { get; set; }
        public List<SimpleIdServer.Core.Common.Models.Client> Clients { get; set; }
        public List<SimpleIdServer.Core.Common.Models.Scope> Scopes { get; set; }
        public List<SimpleIdServer.Core.Common.JsonWebKey> JsonWebKeys { get; set; }
    }

    public class AuthorizationServerOptions
    {
        public AuthorizationServerOptions()
        {
            UmaConfigurationOptions = new Core.UmaConfigurationOptions();
            OAuthConfigurationOptions = new OAuthConfigurationOptions();
            Configuration = new AuthorizationServerConfiguration();            
        }

        public Core.UmaConfigurationOptions UmaConfigurationOptions { get; set; }
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
        public AuthorizationServerConfiguration Configuration { get; set; }
    }
}
