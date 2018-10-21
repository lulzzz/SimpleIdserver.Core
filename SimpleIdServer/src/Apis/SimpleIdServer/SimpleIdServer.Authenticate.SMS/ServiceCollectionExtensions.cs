using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Authenticate.SMS.Actions;
using SimpleIdServer.Authenticate.SMS.Controllers;
using SimpleIdServer.Authenticate.SMS.Services;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Twilio.Client;

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
            services.AddTransient<ITwilioClient, TwilioClient>();
            services.AddTransient<ISmsAuthenticationOperation, SmsAuthenticationOperation>();
            services.AddTransient<IGenerateAndSendSmsCodeOperation, GenerateAndSendSmsCodeOperation>();
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
