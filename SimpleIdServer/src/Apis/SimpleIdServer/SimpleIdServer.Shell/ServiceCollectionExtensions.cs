using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Shell.Controllers;

namespace SimpleIdServer.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicShell(this IServiceCollection services, IMvcBuilder mvcBuilder, ShellModuleOptions shellModuleOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (shellModuleOptions == null)
            {
                throw new ArgumentNullException(nameof(shellModuleOptions));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            services.AddSingleton(shellModuleOptions);
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
