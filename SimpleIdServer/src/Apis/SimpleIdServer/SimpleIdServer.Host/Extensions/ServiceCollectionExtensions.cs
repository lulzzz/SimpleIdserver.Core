using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Bus;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Host.Parsers;
using SimpleIdServer.Logging;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.OpenId.Logging;
using SimpleIdServer.Storage;
using SimpleIdServer.Store;
using System;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdServer.Host.Extensions
{
    public static class ServiceCollectionExtensions 
    {
        public static IServiceCollection AddOpenIdApi(this IServiceCollection serviceCollection, Action<IdentityServerOptions> optionsCallback)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }
            
            var options = new IdentityServerOptions();
            optionsCallback(options);
            serviceCollection.AddOpenIdApi(options);
            return serviceCollection;
        }
        
        /// <summary>
        /// Add the OPENID API.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIdApi(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            ConfigureSimpleIdentityServer(
                serviceCollection, 
                options);
            return serviceCollection;
        }

        public static AuthorizationOptions AddOpenIdSecurityPolicy(this AuthorizationOptions authenticateOptions)
        {
            if (authenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticateOptions));
            }

            authenticateOptions.AddPolicy("Connected", policy => // User is connected
            {
                policy.AddAuthenticationSchemes(Constants.CookieNames.CookieName);
                policy.RequireAuthenticatedUser();
            });
            authenticateOptions.AddPolicy("registration", policy => // Access token with scope = register_client
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim("scope", "register_client");
            });
            authenticateOptions.AddPolicy("connected_user", policy => // Introspect the identity token.
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
            authenticateOptions.AddPolicy("manage_profile", policy => // Access token with scope = manage_profile or with role = administrator
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

                    return claimRoles.Any(s => s.Value == "administrator") || claimScopes.Any(s => s.Value == "manage_profile");
                });
            });
            authenticateOptions.AddPolicy("manage_account_filtering", policy => // Access token with scope = manage_account_filtering or role = administrator
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

                    return claimRoles.Any(s => s.Value == "administrator") || claimScopes.Any(s => s.Value == "manage_account_filtering");
                });
            });
            return authenticateOptions;
        }

        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            IdentityServerOptions options)
        {
            services.AddSimpleIdentityServerCore(options.OAuthConfigurationOptions, 
                clients: options.Configuration.Clients == null ? DefaultConfiguration.DEFAULT_CLIENTS : options.Configuration.Clients,
                resourceOwners: options.Configuration.Users == null ? DefaultConfiguration.DEFAULT_USERS : options.Configuration.Users,
                translations:options.Configuration.Translations == null ? DefaultConfiguration.DEFAULT_TRANSLATIONS : options.Configuration.Translations,
                jsonWebKeys: options.Configuration.JsonWebKeys == null ? null : options.Configuration.JsonWebKeys,
                scopes: DefaultConfiguration.DEFAULT_SCOPES,
                claims: DefaultConfiguration.DEFAULT_CLAIMS,
                passwordSettings: options.Configuration.PasswordSettings == null ? DefaultConfiguration.DEFAULT_PASSWORD_SETTINGS : options.Configuration.PasswordSettings)
                .AddSimpleIdentityServerJwt()
                .AddHostIdentityServer(options)
                .AddDefaultTokenStore()
                .AddDefaultSimpleBus()
                .AddTechnicalLogging()
                .AddOpenidLogging()
                .AddOAuthLogging()
                .AddStorage(o => o.UseInMemoryStorage())
                .AddDataProtection();
        }

        public static IServiceCollection AddHostIdentityServer(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            serviceCollection
                .AddSingleton(options.Scim)
                .AddTransient<IRedirectInstructionParser, RedirectInstructionParser>()
                .AddTransient<IActionResultParser, ActionResultParser>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddDataProtection();
            return serviceCollection;
        }
    }
}