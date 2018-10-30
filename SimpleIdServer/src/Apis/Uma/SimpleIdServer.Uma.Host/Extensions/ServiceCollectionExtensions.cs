﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using SimpleIdServer.Uma.Core.Providers;
using SimpleIdServer.Uma.Host.Configuration;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Uma.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "uma_protection",
                Description = "Access to UMA permission, resource set",
                IsOpenIdScope = false,
                IsDisplayedInConsent = false,
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            }
        };

        public static IServiceCollection AddUmaHost(this IServiceCollection services, AuthorizationServerOptions authorizationServerOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services, authorizationServerOptions);
            return services;
        }

        public static AuthorizationOptions AddUmaSecurityPolicy(this AuthorizationOptions authorizationOptions)
        {
            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            authorizationOptions.AddPolicy("registration", policy => // Access token with scope = register_client
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim("scope", "register_client");
            });
            authorizationOptions.AddPolicy("UmaProtection", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRoles = p.User.Claims.Where(c => c.Type == "role");
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
                authorizationServerOptions.Configuration == null ? null : authorizationServerOptions.Configuration.Resources,
                authorizationServerOptions.Configuration == null ? null : authorizationServerOptions.Configuration.Policies)
                .AddSimpleIdentityServerCore(authorizationServerOptions.OAuthConfigurationOptions,  
                    clients: authorizationServerOptions.Configuration == null ? null : authorizationServerOptions.Configuration.Clients,
                    scopes: authorizationServerOptions.Configuration == null ? DEFAULT_SCOPES : authorizationServerOptions.Configuration.Scopes,
                    jsonWebKeys: authorizationServerOptions.Configuration == null ? null : authorizationServerOptions.Configuration.JsonWebKeys,
                    claims: new List<ClaimAggregate>())
                .AddSimpleIdentityServerJwt()
                .AddDefaultTokenStore()
                .AddDefaultSimpleBus()
                .AddDefaultConcurrency();
            services.AddTechnicalLogging();
            services.AddOAuthLogging();
            services.AddUmaLogging();
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
        }
    }
}