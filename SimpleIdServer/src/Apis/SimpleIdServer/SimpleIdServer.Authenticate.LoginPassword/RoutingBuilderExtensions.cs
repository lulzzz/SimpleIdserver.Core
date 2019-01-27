using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using SimpleIdServer.Authenticate.Basic;

namespace SimpleIdServer.Authenticate.LoginPassword
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseLoginPasswordAuthentication(this IRouteBuilder routeBuilder, LoginPasswordOptions options)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }
            
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            routeBuilder.MapRoute("PasswordAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = Constants.AMR },
                constraints: new { area = Constants.AMR });
            if (options.IsEditCredentialEnabled)
            {
                routeBuilder.MapRoute("EditCredential",
                    "EditCredential/{action}/{id?}",
                    new { controller = "EditCredential", action = "Index", area = Constants.AMR },
                    constraints: new { area = Constants.AMR });
            }

            return routeBuilder;
        }
    }
}
