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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Bus;
using SimpleIdServer.Client;
using SimpleIdServer.Concurrency;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Logging;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;
using SimpleIdServer.Uma.Core;
using SimpleIdServer.Uma.Host.Controllers;
using SimpleIdServer.Uma.Host.Middlewares;
using SimpleIdServer.Uma.Host.Tests.MiddleWares;
using SimpleIdServer.Uma.Host.Tests.Services;
using SimpleIdServer.Uma.Host.Tests.Stores;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;

namespace SimpleIdServer.Uma.Host.Tests.Fakes
{
    public class FakeUmaStartup : IStartup
    {
        public const string DefaultSchema = JwtBearerDefaults.AuthenticationScheme;
        private SharedContext _context;

        public FakeUmaStartup(SharedContext context)
        {
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services);
            // 2. Add authorization policies.
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            })
            .AddFakeCustomAuth(o => { });
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("permissions", policy =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAssertion(p => true);
                });
                opts.AddPolicy("policies", policy =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAssertion(p => true);
                });
                opts.AddPolicy("register_client", policy =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAssertion(p => true);
                });
                opts.AddPolicy("resources", policy =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAssertion(p => true);
                });
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication.
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API.
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(ConfigurationController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use(async (context, next) =>
            {
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("client_id", "resource_server")
                }, "fakests");
                context.User = new ClaimsPrincipal(claimsIdentity);
                await next.Invoke();
            });

            // 3. Enable CORS
            app.UseCors("AllowAll");
            // 4. Display exception
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
            // 5. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            // 1. Add CORE.
            services.AddSimpleIdServerUmaCore(null, UmaStores.GetResources())
                .AddSimpleIdentityServerCore(clients: OAuthStores.GetClients(), jsonWebKeys: OAuthStores.GetJsonWebKeys(_context), scopes: OAuthStores.GetScopes())
                .AddSimpleIdentityServerJwt()
                .AddTransient<IIdentityServerClientFactory, IdentityServerClientFactory>()
                .AddDefaultSimpleBus()
                .AddDefaultConcurrency()
                .AddDefaultTokenStore();

            // 3. Enable logging.
            services.AddSingleton(new AuthorizationServerOptions
            {
                OpenidWellKnownConfiguration = "http://localhost:60000/.well-known/openid-configuration"
            });
            services.AddLogging();
            services.AddTechnicalLogging();
            services.AddOAuthLogging();
            services.AddUmaLogging();
            // 4. Register the services.
            services.AddTransient<SimpleIdServer.Core.Services.IConfigurationService, DefaultConfigurationService>();
            // 5. Register other classes.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
            services.AddTransient<IIdentityServerClientFactory, FakeIdentityServerClientFactory>();
        }
    }
}