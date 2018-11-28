using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Uma.Website.Host.Controllers;
using System;

namespace SimpleIdServer.Uma.Website.Host
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmaWebsiteStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "SimpleIdServer.Uma.Website.Host.wwwroot");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider
            });
            return app;
        }
    }
}
