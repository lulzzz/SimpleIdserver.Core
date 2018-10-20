using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Shell.Controllers;

namespace SimpleIdServer.Shell
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseShellStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "SimpleIdentityServer.Shell.wwwroot");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider
            });
            return app;
        }
    }
}
