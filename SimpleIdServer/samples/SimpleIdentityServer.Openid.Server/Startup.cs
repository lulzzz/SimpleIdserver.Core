#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Authenticate.LoginPassword;
using SimpleIdServer.Host;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.Shell;
using SimpleIdServer.UserManagement;

namespace SimpleIdServer.Openid.Server
{
    public class Startup
    {
        private IdentityServerOptions _options;
        private IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _options = new IdentityServerOptions
            {
                Configuration = new OpenIdServerConfiguration
                {
                    Users = DefaultConfiguration.GetUsers(),
                    JsonWebKeys = DefaultConfiguration.GetJsonWebKeys(),
                    Clients = DefaultConfiguration.GetClients()
                }
            };
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddLogging();
            services.AddAuthentication(Constants.CookieNames.CookieName)
                .AddCookie(Constants.CookieNames.CookieName, opts =>
                {
                    opts.LoginPath = "/Authenticate";
                });
            services.AddAuthorization(opts =>
            {
                opts.AddOpenIdSecurityPolicy(Constants.CookieNames.CookieName);
            });
            // 5. Configure MVC
            var mvcBuilder = services.AddMvc();
            services.AddOpenIdApi(_options); // API
            services.AddBasicShell(mvcBuilder);  // SHELL
            services.AddLoginPasswordAuthentication(mvcBuilder, new BasicAuthenticateOptions());  // LOGIN & PASSWORD
            services.AddUserManagement(mvcBuilder);  // USER MANAGEMENT
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseShellStaticFiles();
            // 3. Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");
            // 4. Enable SimpleIdentityServer
            app.UseOpenIdApi(_options, loggerFactory);
            // 5. Configure ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.UseLoginPasswordAuthentication();
                routes.MapRoute("AuthArea",
                    "{area:exists}/Authenticate/{action}/{id?}",
                    new { controller = "Authenticate", action = "Index" });
                routes.UseUserManagement();
                routes.UseShell();
            });
        }
    }
}