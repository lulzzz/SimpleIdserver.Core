using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Module;
using SimpleIdServer.UserManagement.Controllers;
using System;

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

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            services.AddSingleton(userManagementOptions);
            services.AddSingleton<IUiModule>(new UiManagerModuleUI());
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }

        private class UiManagerModuleUI : IUiModule
        {
            public string DisplayName { get => "Profile"; }
            public string Name { get => "uimanager"; }
            public string Picture { get => ""; }
            public RedirectUrl RedirectionUrl => new RedirectUrl
            {
                ActionName = "Index",
                ControllerName = "Home",
                Area = "admin"
            };
        }
    }
}
