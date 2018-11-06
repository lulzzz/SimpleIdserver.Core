using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Bus;
using SimpleIdServer.Concurrency;
using SimpleIdServer.Scim.Core;
using System;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdServer.Scim.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimHost(this IServiceCollection services, ScimServerOptions scimServerOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            RegisterServices(services, scimServerOptions);
            return services;
        }

        public static AuthorizationOptions AddScimAuthPolicy(this AuthorizationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AddPolicy("scim_manage", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRoles = p.User.Claims.Where(c => c.Type == ClaimTypes.Role);
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (!claimRoles.Any() && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRoles.Any(c => c.Value == "administrator") || claimScopes.Any(c => c.Value == "scim_manage");
                });
            });
            options.AddPolicy("scim_read", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRoles = p.User.Claims.Where(c => c.Type == ClaimTypes.Role);
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (!claimRoles.Any() && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRoles.Any(c => c.Value == "administrator") || claimScopes.Any(c => c.Value == "scim_read");
                });
            });
            options.AddPolicy("authenticated", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
            return options;
        }

        private static void RegisterServices(IServiceCollection services, ScimServerOptions scimServerOptions)
        {
            services.AddScimCore(scimServerOptions.ServerConfiguration == null ? null : scimServerOptions.ServerConfiguration.Representations,
                scimServerOptions.ServerConfiguration == null ? null : scimServerOptions.ServerConfiguration.Schemas)
                .AddDefaultSimpleBus()
                .AddDefaultConcurrency();
        }
    }
}
