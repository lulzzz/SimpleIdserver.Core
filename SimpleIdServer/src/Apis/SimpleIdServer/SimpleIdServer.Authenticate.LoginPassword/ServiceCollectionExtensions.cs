using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Authenticate.LoginPassword.Actions;
using SimpleIdServer.Authenticate.LoginPassword.Controllers;
using SimpleIdServer.Authenticate.LoginPassword.Services;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Module;
using System;
using System.Reflection;

namespace SimpleIdServer.Authenticate.LoginPassword
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoginPasswordAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, LoginPasswordOptions basicAuthenticateOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (basicAuthenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(basicAuthenticateOptions));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedFileProvider);
                opts.CompilationCallback = (context) =>
                {
                    var assm = MetadataReference.CreateFromFile(Assembly.Load("SimpleIdServer.Authenticate.Basic").Location);
                    context.Compilation = context.Compilation.AddReferences(assm);
                };
            });
            services.AddSingleton(basicAuthenticateOptions);
            services.AddTransient<IAuthenticateResourceOwnerService, PasswordAuthenticateResourceOwnerService>();
            services.AddTransient<IChangePasswordAction, ChangePasswordAction>();
            services.AddTransient<ILoginPwdAuthenticateAction, LoginPwdAuthenticateAction>();
            services.AddAuthBasic();
            services.AddSingleton<IAuthModule>(new PwdAuthModule(basicAuthenticateOptions.IsEditCredentialEnabled));
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }

        private class PwdAuthModule : IAuthModule
        {
            private readonly bool _isEnabled;

            public PwdAuthModule(bool isEnabled)
            {
                _isEnabled = isEnabled;
            }

            public string Name { get => Constants.AMR; }
            public string DisplayName { get => "login & password"; }
            public bool IsEditCredentialsEnabled { get => _isEnabled; }
            public RedirectUrl EditCredentialUrl
            {
                get => new RedirectUrl
                {
                    ActionName = "Index",
                    ControllerName = "EditCredential",
                    Area = Constants.AMR
                };
            }
            public RedirectUrl ConfigurationUrl
            {
                get => new RedirectUrl
                {
                    ActionName = "Index",
                    ControllerName = "Configuration",
                    Area = Constants.AMR
                };
            }
        }
    }
}
