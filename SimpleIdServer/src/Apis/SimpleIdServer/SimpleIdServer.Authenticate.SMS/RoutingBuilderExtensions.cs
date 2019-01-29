﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleIdServer.Authenticate.SMS
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseSmsAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }
            
            routeBuilder.MapRoute("BasicAuthentication",
                Constants.AMR + "/Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = Constants.AMR },
                constraints: new { area = Constants.AMR });
            return routeBuilder;
        }
    }
}
