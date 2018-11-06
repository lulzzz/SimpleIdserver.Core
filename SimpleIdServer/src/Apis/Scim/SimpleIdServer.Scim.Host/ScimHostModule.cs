using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SimpleIdServer.Lib;
using SimpleIdServer.Module;
using SimpleIdServer.Scim.Host.Controllers;
using SimpleIdServer.Scim.Host.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdServer.Scim.Host
{
    public class ScimHostModule : IModule
    {
        public const string OPENID_STORE_LOCATION_NAME = "OpenIdStoreLocation";
        public const string OPENID_FIND_BY_SUBJECT_DISTINGUISHED_NAME = "OpenIdFindBySubjectDistinguishedName";
        public const string OAUTH_STORE_LOCATION_NAME = "OAuthdStoreLocation";
        public const string OAUTH_FIND_BY_SUBJECT_DISTINGUISHED_NAME = "OAuthFindBySubjectDistinguishedName";
        public const string OAUTH_ISSUER = "OauthIssuer";
        public const string OPENID_ISSUER = "OpenIdIssuer";
        public const string DEFAULT_OPENID_ISSUER = "http://localhost:60000";
        public const string DEFAULT_OAUTH_ISSUER = "http://localhost:60004";
        private static Dictionary<string, StoreLocation> _mappingStrToStoreLocation = new Dictionary<string, StoreLocation>
        {
            { "CurrentUser", StoreLocation.CurrentUser },
            { "LocalMachine", StoreLocation.LocalMachine }
        };
        public const string OAUTH_PUBLIC_KEY_FILE_NAME = "oauth_puk.txt";
        public const string OPENID_PUBLIC_KEY_FILE_NAME = "openid_puk.txt";
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties == null ? new Dictionary<string, string>() : properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthenticationAdded += HandleAuthenticationAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
        }

        #region Handle events

        private void HandleAuthenticationAdded(object sender, EventArgs e)
        {
            var oauthSecurityKey = GetSecurityKey(OAUTH_STORE_LOCATION_NAME, OAUTH_FIND_BY_SUBJECT_DISTINGUISHED_NAME, OAUTH_PUBLIC_KEY_FILE_NAME);
            var openidSecurityKey = GetSecurityKey(OPENID_STORE_LOCATION_NAME, OPENID_FIND_BY_SUBJECT_DISTINGUISHED_NAME, OPENID_PUBLIC_KEY_FILE_NAME);
            var openidIssuer = DEFAULT_OPENID_ISSUER;
            var oauthIssuer = DEFAULT_OAUTH_ISSUER;
            if (_properties.ContainsKey(OAUTH_ISSUER))
            {
                oauthIssuer = _properties[OAUTH_ISSUER];
            }

            if (_properties.ContainsKey(OPENID_ISSUER))
            {
                openidIssuer = _properties[OPENID_ISSUER];
            }

            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
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
                        oauthIssuer,
                        openidIssuer
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
            });
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddScimHost(new ScimServerOptions());
        }
		
        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            var mvcBuilder = AspPipelineContext.Instance().ConfigureServiceContext.MvcBuilder;
            var scimAssembly = typeof(SchemasController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(scimAssembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(scimAssembly);
        }

        private void HandleAuthorizationAdded(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddScimAuthPolicy();
        }

        #endregion

        #region Get certificate

        private RsaSecurityKey GetSecurityKey(string locationName, string subDistName, string txtFileName)
        {
            string xml = string.Empty;
            if (_properties.ContainsKey(locationName) && _properties.ContainsKey(subDistName))
            {
                var storeLocation = _properties[locationName];
                if (_mappingStrToStoreLocation.ContainsKey(storeLocation))
                {
                    using (var store = new X509Store(_mappingStrToStoreLocation[storeLocation]))
                    {
                        store.Open(OpenFlags.OpenExistingOnly);
                        var certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, _properties[subDistName], true);
                        if (certificates.Count > 0)
                        {
                            xml = ((RSACryptoServiceProvider)certificates[0].PrivateKey).ToXmlStringNetCore(false);
                        }
                    }
                }
            }
            else
            {
                var locationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var publicKeyLocationPath = Path.Combine(locationPath, txtFileName);
                xml = File.ReadAllText(publicKeyLocationPath);
            }

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