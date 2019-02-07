using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Authenticate.SMS.Actions;
using SimpleIdServer.Authenticate.SMS.Controllers;
using SimpleIdServer.Authenticate.SMS.Services;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Module;
using SimpleIdServer.Twilio.Client;
using System;
using System.Reflection;

namespace SimpleIdServer.Authenticate.SMS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmsAuthentication(this IServiceCollection services,  IMvcBuilder mvcBuilder, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (smsAuthenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(smsAuthenticationOptions));
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
            services.AddSingleton(smsAuthenticationOptions);
            services.AddAuthBasic();
            services.AddTransient<ITwilioClient, TwilioClient>();
            services.AddTransient<ISmsAuthenticationOperation, SmsAuthenticationOperation>();
            services.AddTransient<IGenerateAndSendSmsCodeOperation, GenerateAndSendSmsCodeOperation>();
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            services.AddSingleton<IAuthModule>(new SmsAuthModule());
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }

        private class SmsAuthModule : IAuthModule
        {
            public SmsAuthModule()
            {
            }

            public string Name { get => Constants.AMR; }
            public string DisplayName { get => "sms"; }
            public bool IsEditCredentialsEnabled { get => false; }
            public RedirectUrl EditCredentialUrl
            {
                get => null;
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
