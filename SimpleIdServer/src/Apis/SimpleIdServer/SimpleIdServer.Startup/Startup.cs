using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdServer.Authenticate.LoginPassword;
using SimpleIdServer.Authenticate.SMS;
using SimpleIdServer.Host;
using SimpleIdServer.IdentityStore.LDAP;
using SimpleIdServer.Module;
using SimpleIdServer.Shell;
using SimpleIdServer.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleIdServer.Startup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            var refAssms = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            var rl = Microsoft.Extensions.DependencyModel.DependencyContext.Default.RuntimeLibraries.FirstOrDefault(r => r.Name == "SimpleIdServer.Authenticate.LoginPassword");
            var loginAssm = Assembly.Load(new AssemblyName(rl.Name));
            var type = loginAssm.GetExportedTypes().Where(t => typeof(IModule).IsAssignableFrom(t));
            var loginPasswordModule = (IModule)Activator.CreateInstance(type.First());

            var simpleIdServerModule = new SimpleIdentityServerHostModule();
            var shellModule = new ShellModule();
            // var loginPasswordModule = new LoginPasswordModule();
           var smsModule = new SmsModule();
           var userManagementModule = new UserManagementModule();
           simpleIdServerModule.Init(null);
           shellModule.Init(null);
           loginPasswordModule.Init(new Dictionary<string, string>
           {
               { "IsEditCredentialEnabled", "true" }
           });
           smsModule.Init(new Dictionary<string, string>
           {
               { "IsSelfProvisioningEnabled", "true" }
           });
           userManagementModule.Init(new Dictionary<string, string>
           {
               { "CanUpdateTwoFactorAuthentication", "true" }
           });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddLogging();
            var mvcBuilder = services.AddMvc();
            AspPipelineContext.Instance().StartConfigureServices(services);
            services.AddAuthentication(Constants.CookieNames.ExternalCookieName)
                .AddCookie(Constants.CookieNames.ExternalCookieName)
                .AddCookie(Constants.CookieNames.ChangePasswordCookieName)
                .AddCookie(Constants.CookieNames.PasswordLessCookieName)
                .AddCookie(Constants.CookieNames.AcrCookieName)
                .AddFacebook(opts =>
                {
                    opts.ClientId = "569242033233529";
                    opts.ClientSecret = "12e0f33817634c0a650c0121d05e53eb";
                    opts.SignInScheme = Host.Constants.CookieNames.ExternalCookieName;
                    opts.Scope.Add("public_profile");
                    opts.Scope.Add("email");
                });
            services.AddAuthentication(Host.Constants.CookieNames.CookieName)
                .AddCookie(Host.Constants.CookieNames.CookieName, opts =>
                {
                    opts.LoginPath = "/Home/Authenticate";
                });

            AspPipelineContext.Instance().ConfigureServiceContext.AddAuthentication();
            AspPipelineContext.Instance().ConfigureServiceContext.AddMvc(mvcBuilder);
            services.AddAuthorization(opts =>
            {
                AspPipelineContext.Instance().ConfigureServiceContext.AddAuthorization(opts);
            });

            services.AddIdentityStoreLDAP(new IdentityStoreLDAPOptions
            {
                Port = 389,
                UserName = "cn=thabart,cn=users,cn=system",
                Password = "password",
                Server = "127.0.0.1",
                LDAPBaseDN = "cn=system",
                LDAPFilterUser = "(cn={0})"
            });
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            AspPipelineContext.Instance().StartConfigureApplicationBuilder(app, env, loggerFactory);
            app.UseCors("AllowAll");
            loggerFactory.AddConsole();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                AspPipelineContext.Instance().ApplicationBuilderContext.ConfigureRoutes(routes);
                routes.MapRoute("AuthArea",
                    "{area:exists}/Authenticate/{action}/{id?}",
                    new { controller = "Authenticate", action = "Index" });
            });
        }
    }
}