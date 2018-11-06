using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdServer.Module;
using SimpleIdServer.Uma.Host;

namespace SimpleIdServer.Uma.Startup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var umaHostModule = new UmaHostModule();
            UmaHostModule.NewCertificate();
            umaHostModule.Init(null);
        }
                
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            var mvcBuilder = services.AddMvc();
            AspPipelineContext.Instance().StartConfigureServices(services);
            AspPipelineContext.Instance().ConfigureServiceContext.AddAuthentication();
            AspPipelineContext.Instance().ConfigureServiceContext.AddMvc(mvcBuilder);
            services.AddAuthorization(opts =>
            {
                AspPipelineContext.Instance().ConfigureServiceContext.AddAuthorization(opts);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            AspPipelineContext.Instance().StartConfigureApplicationBuilder(app, env, loggerFactory);
            app.UseCors("AllowAll");
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                AspPipelineContext.Instance().ApplicationBuilderContext.ConfigureRoutes(routes);
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }
    }
}