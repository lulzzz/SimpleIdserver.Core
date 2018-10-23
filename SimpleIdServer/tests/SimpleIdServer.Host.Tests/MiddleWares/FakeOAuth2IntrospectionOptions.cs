using Microsoft.AspNetCore.Authentication;
using SimpleIdServer.Client;

namespace SimpleIdServer.Host.Tests.MiddleWares
{
    public class FakeOAuth2IntrospectionOptions : AuthenticationSchemeOptions
    {
        public FakeOAuth2IntrospectionOptions()
        {
            IdentityServerClientFactory = new IdentityServerClientFactory();
        }

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IIdentityServerClientFactory IdentityServerClientFactory { get; set; }
    }
}
