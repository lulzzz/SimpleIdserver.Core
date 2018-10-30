using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Lib;
using SimpleIdServer.Module;
using SimpleIdServer.Uma.Host.Controllers;
using SimpleIdServer.Uma.Host.Extensions;
using SimpleIdServer.Uma.Host.Middlewares;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdServer.Uma.Host
{
    public class UmaHostModule : IModule
    {
        private const string STORE_LOCATION_NAME = "StoreLocation";
        private const string FIND_BY_SUBJECT_DISTINGUIDES_NAME = "FindBySubjectDistinguishedName";
        private static Dictionary<string, StoreLocation> _mappingStrToStoreLocation = new Dictionary<string, StoreLocation>
        {
            { "CurrentUser", StoreLocation.CurrentUser },
            { "LocalMachine", StoreLocation.LocalMachine }
        };
        private IDictionary<string, string> _properties;
        private AuthorizationServerOptions _options;

        #region Public methods

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthenticationAdded += HandleAuthenticationAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
        }

        #endregion

        #region Handle events

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            _options = BuildOptions(_properties);
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            services.AddUmaHost(_options);
        }

        private void HandleAuthenticationAdded(object sender, EventArgs e)
        {
            RsaSecurityKey rsa = null;
            var xml = _options.Configuration.JsonWebKeys.First().SerializedKey;
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

            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = rsa
                };
            });
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            var mvcBuilder = AspPipelineContext.Instance().ConfigureServiceContext.MvcBuilder;
            var umaAssembly = typeof(JwksController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(umaAssembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(umaAssembly);
        }

        private void HandleAuthorizationAdded(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddUmaSecurityPolicy();
        }

        private void HandleApplicationBuilderInitialized(object sender, EventArgs e)
        {
            var app = AspPipelineContext.Instance().ApplicationBuilderContext.App;
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
        }

        #endregion

        #region Private methods

        private static AuthorizationServerOptions BuildOptions(IDictionary<string, string> properties)
        {
            var result = new AuthorizationServerOptions();
            if (properties != null)
            {
                string xml = string.Empty;
                if (properties.ContainsKey(STORE_LOCATION_NAME) && properties.ContainsKey(FIND_BY_SUBJECT_DISTINGUIDES_NAME))
                {
                    var storeLocation = properties[STORE_LOCATION_NAME];
                    if (_mappingStrToStoreLocation.ContainsKey(storeLocation))
                    {
                        using (var store = new X509Store(_mappingStrToStoreLocation[storeLocation]))
                        {
                            store.Open(OpenFlags.OpenExistingOnly);
                            var certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, properties[FIND_BY_SUBJECT_DISTINGUIDES_NAME], true);
                            if (certificates.Count > 0)
                            {
                                xml = ((RSACryptoServiceProvider)certificates[0].PrivateKey).ToXmlStringNetCore(true);
                            }
                        }
                    }
                }
                else
                {
                    var locationPath = GetLocationPath();
                    var privateKeyLocationPath = Path.Combine(locationPath, "prk.txt");
                    if (!File.Exists(privateKeyLocationPath))
                    {
                        NewCertificate();
                    }

                    xml = File.ReadAllText(privateKeyLocationPath);
                }

                if (!string.IsNullOrWhiteSpace(xml))
                {
                    result.Configuration.JsonWebKeys = new List<SimpleIdServer.Core.Common.JsonWebKey>
                    {
                        new SimpleIdServer.Core.Common.JsonWebKey // Private key is used to sign
                        {
                            Alg = AllAlg.RS256,
                            KeyOps = new[]
                                {
                                    KeyOperations.Sign,
                                    KeyOperations.Verify
                                },
                            Kid = "1",
                            Kty = KeyType.RSA,
                            Use = Use.Sig,
                            SerializedKey = xml
                        },
                        new SimpleIdServer.Core.Common.JsonWebKey // Private key is used to decrypt
                        {
                            Alg = AllAlg.RSA1_5,
                            KeyOps = new []
                            {
                                KeyOperations.Encrypt,
                                KeyOperations.Decrypt
                            },
                            Kid = "2",
                            Kty = KeyType.RSA,
                            Use = Use.Enc,
                            SerializedKey = xml
                        }
                    };
                }
            }

            return result;
        }

        private static void NewCertificate()
        {
            var privateSerializedRsa = string.Empty;
            var publicSerializedRsa = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var provider = new RSACryptoServiceProvider())
                {
                    privateSerializedRsa = provider.ToXmlStringNetCore(true);
                    publicSerializedRsa = provider.ToXmlStringNetCore(false);
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    privateSerializedRsa = rsa.ToXmlStringNetCore(true);
                    publicSerializedRsa = rsa.ToXmlStringNetCore(false);
                }
            }

            var locationPath = GetLocationPath();
            var publicKeyFilePath = Path.Combine(locationPath, "puk.txt");
            var privateKeyFilePath = Path.Combine(locationPath, "prk.txt");
            if (File.Exists(publicKeyFilePath))
            {
                File.Delete(publicKeyFilePath);
            }

            if (File.Exists(privateKeyFilePath))
            {
                File.Delete(privateKeyFilePath);
            }

            File.WriteAllText(publicKeyFilePath, publicSerializedRsa);
            File.WriteAllText(privateKeyFilePath, privateSerializedRsa);
        }

        private static string GetLocationPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        #endregion
    }
}