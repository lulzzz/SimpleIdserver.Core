﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

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
                Constants.AMR + "/Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = Constants.AMR },
                constraints: new { area = Constants.AMR });
            if (options.IsEditCredentialEnabled)
            {
                routeBuilder.MapRoute("PwdEditCredential",
                    Constants.AMR + "/EditCredential/{action}/{id?}",
                    new { controller = "EditCredential", action = "Index", area = Constants.AMR },
                    constraints: new { area = Constants.AMR });
            }

            routeBuilder.MapRoute("PwdConfiguration",
                Constants.AMR + "/Configuration",
                new { controller = "Configuration", action = "Index", area = Constants.AMR },
                constraints: new { area = Constants.AMR });
            routeBuilder.MapRoute("PwdEditCredentialApi",
                Constants.AMR + "/Credentials/{action}/{id?}",
                new { controller = "Credentials", action = "Index", area = Constants.AMR },
                constraints: new { area = Constants.AMR });
            return routeBuilder;
        }
    }
}
