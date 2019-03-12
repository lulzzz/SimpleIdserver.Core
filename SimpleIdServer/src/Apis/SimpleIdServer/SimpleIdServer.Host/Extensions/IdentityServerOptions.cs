using SimpleIdServer.Core;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;

namespace SimpleIdServer.Host.Extensions
{
    public class ScimOptions
    {
        public string EndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class OpenIdServerConfiguration
    {
        public List<User> Users { get; set; }
        public List<Client> Clients { get; set; }
        public List<Translation> Translations { get; set; }
        public List<JsonWebKey> JsonWebKeys { get; set; }
        public List<CredentialSetting> CredentialSettings{ get; set; }
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Scim = new ScimOptions();
            Configuration = new OpenIdServerConfiguration();
        }

        /// <summary>
        /// Scim options.
        /// </summary>
        public ScimOptions Scim { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public OpenIdServerConfiguration Configuration { get; set; }
        /// <summary>
        /// Gets or sets the OAUTH configuration options.
        /// </summary>
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
    }
}
