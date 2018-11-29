using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Uma.Core;
using SimpleIdServer.Uma.Website.Host.Controllers;
using System;

namespace SimpleIdServer.Uma.Website.Host
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaWebsite(this IServiceCollection services, IMvcBuilder mvcBuilder, UmaWebsiteOptions umaWebsiteOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (umaWebsiteOptions == null)
            {
                throw new ArgumentNullException(nameof(umaWebsiteOptions));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
            services.AddSingleton(umaWebsiteOptions.Authentication);
            services.AddSimpleIdServerUmaWebsite(umaWebsiteOptions.Configuration.ResourceSet, null, umaWebsiteOptions.Configuration.Policies);
            return services;
        }

        public static AuthorizationOptions AddUmaWebsiteSecurityPolicy(this AuthorizationOptions authorizationOptions)
        {
            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            authorizationOptions.AddPolicy("connected", policy =>
            {
                policy.AddAuthenticationSchemes(Constants.DEFAULT_COOKIE_NAME);
                policy.RequireAuthenticatedUser();
            });
            return authorizationOptions;
        }
    }
}