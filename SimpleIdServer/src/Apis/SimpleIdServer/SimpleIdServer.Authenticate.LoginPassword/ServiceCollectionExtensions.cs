using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Authenticate.LoginPassword.Controllers;
using SimpleIdServer.Authenticate.LoginPassword.Services;
using SimpleIdServer.Core.Services;

namespace SimpleIdServer.Authenticate.LoginPassword
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoginPasswordAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, BasicAuthenticateOptions basicAuthenticateOptions)
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
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
