using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.UserManagement.Controllers;

namespace SimpleIdServer.UserManagement
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserManagement(this IServiceCollection services, IMvcBuilder mvcBuilder, UserManagementOptions userManagementOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (userManagementOptions == null)
            {
                throw new ArgumentNullException(nameof(userManagementOptions));
            }

            var assembly = typeof(UserController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            services.AddSingleton(userManagementOptions);
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
