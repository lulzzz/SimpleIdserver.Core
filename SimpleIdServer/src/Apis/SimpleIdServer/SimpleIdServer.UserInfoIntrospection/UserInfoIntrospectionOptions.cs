using Microsoft.AspNetCore.Authentication;
using SimpleIdServer.Common.Client.Factories;

namespace SimpleIdServer.UserInfoIntrospection
{
    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
   {
        public const string AuthenticationScheme = "UserInfoIntrospection";

        public string WellKnownConfigurationUrl { get; set; }
        public IHttpClientFactory HttpClientFactory { get; set; }
    }
}
