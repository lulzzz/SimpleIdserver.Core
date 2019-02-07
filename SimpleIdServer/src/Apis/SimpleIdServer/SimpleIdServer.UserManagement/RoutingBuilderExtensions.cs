using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleIdServer.UserManagement
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseUserManagement(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("UserManagement",
                "admin/{controller}/{action}/{id?}",
                new { controller = "Home", action = "Index", area = "admin" },
                constraints: new { area = "admin" });
            return routeBuilder;
        }
    }
}
