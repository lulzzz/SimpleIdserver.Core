using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleIdServer.Lib;
using SimpleIdServer.Module;
using SimpleIdServer.Uma.Host;
using SimpleIdServer.Uma.Host.Extensions;
using SimpleIdServer.Uma.Startup.Extensions;
using SimpleIdServer.Uma.Website.Host;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SimpleIdServer.Uma.Startup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            _options = new AuthorizationServerOptions
            {
                Configuration = new AuthorizationServerConfiguration
                {
                    JsonWebKeys = DefaultConfiguration.GetJsonWebKeys()
                }
            };
        }

        private AuthorizationServerOptions _options;

        #region Public methods

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            ConfigureSecurity(services);
            services.AddAuthorization(opts =>
            {
                opts.AddUmaSecurityPolicy();
                opts.AddUmaWebsiteSecurityPolicy();
            });
            var mvc = services.AddMvc();
            services.AddUmaHost(_options, mvc);
            services.AddUmaWebsite(mvc, new UmaWebsiteOptions
            {
                Authentication = new UmaAuthenticationWebsiteOptions
                {
                    OpenidWellKnownConfigurationUrl = "http://localhost:60000/.well-known/openid-configuration",
                    ClientId = "uma",
                    ClientSecret = "umaSecret"
                }
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            AspPipelineContext.Instance().StartConfigureApplicationBuilder(app, env, loggerFactory);
            app.UseCors("AllowAll");
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            app.UseAuthentication();
            app.UseUmaWebsiteStaticFiles();
            app.UseMvc(routes =>
            {
                AspPipelineContext.Instance().ApplicationBuilderContext.ConfigureRoutes(routes);
                routes.MapRoute("default",
                    "{controller}/{action}/{id?}",
                    new { controller = "Home", action = "Index" });
            });
        }

        #endregion

        #region Private methods

        private void ConfigureSecurity(IServiceCollection services)
        {
            var oauthSecurityKey = GetSecurityKey("puk.txt");
            var openidSecurityKey = GetSecurityKey("openid_puk.txt");
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
                   ValidIssuers = new List<string>
                   {
                        "http://localhost:60004",
                        "https://localhost:60014",
                        "http://localhost:60000",
                        "https://localhost:60010"
                   },
                   ValidateAudience = false,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                   {
                       List<SecurityKey> keys = new List<SecurityKey>
                       {
                            oauthSecurityKey,
                            openidSecurityKey
                       };
                       return keys;
                   }
               };
           }).AddCookie(Website.Host.Constants.DEFAULT_COOKIE_NAME, opts =>
           {
               opts.LoginPath = "/Authenticate";
           });
        }

        private RsaSecurityKey GetSecurityKey(string txtFileName)
        {
            var locationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var publicKeyLocationPath = Path.Combine(locationPath, txtFileName);
            var xml = File.ReadAllText(publicKeyLocationPath);
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

            return rsa;
        }

        #endregion
    }
}