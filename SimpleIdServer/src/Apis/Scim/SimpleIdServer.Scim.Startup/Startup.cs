using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdServer.Module;
using SimpleIdServer.Scim.Host;
using SimpleIdServer.Scim.Host.Extensions;

namespace SimpleIdServer.Scim.Startup
{

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var scimHostModule = new ScimHostModule();
            scimHostModule.Init(null);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            var mvcBuilder = services.AddMvc();
            AspPipelineContext.Instance().StartConfigureServices(services);
            AspPipelineContext.Instance().ConfigureServiceContext.AddMvc(mvcBuilder);
            AspPipelineContext.Instance().ConfigureServiceContext.AddAuthentication();
            services.AddAuthorization(opts =>
            {
                opts.AddScimAuthPolicy();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAll");
            loggerFactory.AddConsole();
            app.UseAuthentication();
            app.UseStatusCodePages();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
