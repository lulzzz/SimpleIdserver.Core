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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleIdServer.Lib;
using SimpleIdServer.Uma.Host;
using SimpleIdServer.Uma.Host.Extensions;
using SimpleIdServer.Uma.Host.Middlewares;
using SimpleIdServer.Uma.Logging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SimpleIdServer.Uma.Startup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _options = new AuthorizationServerOptions
            {
                Configuration = new AuthorizationServerConfiguration
                {
                    JsonWebKeys = DefaultConfiguration.GetJsonWebKeys()
                }
            };
        }

        private AuthorizationServerOptions _options;

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var xml = _options.Configuration.JsonWebKeys.First().SerializedKey;
            RsaSecurityKey rsa = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var provider = new RSACryptoServiceProvider();
                provider.FromXmlStringNetCore(xml);
                rsa = new RsaSecurityKey(provider);
            }
            else
            {
                var r = new RSAOpenSsl();
                r.FromXmlStringNetCore(xml);
                rsa = new RsaSecurityKey(r);
            }

            services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = "http://localhost:60004",
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = rsa
                };
            });
            services.AddAuthorization(opts =>
            {
                opts.AddUmaSecurityPolicy();
            });
            services.AddLogging();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddUmaHost(_options);
	        services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            app.UseAuthentication();
            app.UseCors("AllowAll");
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }
    }
}