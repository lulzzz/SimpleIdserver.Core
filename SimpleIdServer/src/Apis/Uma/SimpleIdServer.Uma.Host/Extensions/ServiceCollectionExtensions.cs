using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Bus;
using SimpleIdServer.Concurrency;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Logging;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;
using SimpleIdServer.Uma.Core;
using SimpleIdServer.Uma.Host.Controllers;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdServer.Uma.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaHost(this IServiceCollection services, AuthorizationServerOptions authorizationServerOptions, IMvcBuilder mvcBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            RegisterServices(services, authorizationServerOptions);
            return services;
        }

        public static AuthorizationOptions AddUmaSecurityPolicy(this AuthorizationOptions authorizationOptions)
        {
            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            authorizationOptions.AddPolicy("register_client", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim("scope", "register_client");
            });
            authorizationOptions.AddPolicy("uma_protection", policy =>
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

                    return claimRoles.Any(s => s.Value == "administrator") || claimScopes.Any(s => s.Value == "uma_protection");
                });
            });
            return authorizationOptions;
        }

        private static void RegisterServices(IServiceCollection services, AuthorizationServerOptions authorizationServerOptions)
        {
            services.AddSimpleIdServerUmaCore(authorizationServerOptions.UmaConfigurationOptions, 
                authorizationServerOptions.Configuration.Resources == null ? null : authorizationServerOptions.Configuration.Resources,
                authorizationServerOptions.Configuration.Policies == null ? null : authorizationServerOptions.Configuration.Policies)
                .AddSimpleIdentityServerCore(authorizationServerOptions.OAuthConfigurationOptions,  
                    clients: authorizationServerOptions.Configuration.Clients == null ? DefaultConfiguration.DEFAULT_CLIENTS : authorizationServerOptions.Configuration.Clients,
                    scopes: authorizationServerOptions.Configuration.Scopes == null ? DefaultConfiguration.DEFAULT_SCOPES : authorizationServerOptions.Configuration.Scopes,
                    jsonWebKeys: authorizationServerOptions.Configuration.JsonWebKeys == null ? null : authorizationServerOptions.Configuration.JsonWebKeys,
                    claims: new List<ClaimAggregate>())
                .AddSimpleIdentityServerJwt()
                .AddDefaultTokenStore()
                .AddDefaultSimpleBus()
                .AddDefaultConcurrency();
            services.AddTechnicalLogging();
            services.AddOAuthLogging();
            services.AddUmaLogging();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
        }
    }
}