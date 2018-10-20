using Microsoft.AspNetCore.Authentication;

namespace SimpleIdServer.Host.Tests.MiddleWares
{
    public class FakeUserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "UserInfoIntrospection";
    }
}
