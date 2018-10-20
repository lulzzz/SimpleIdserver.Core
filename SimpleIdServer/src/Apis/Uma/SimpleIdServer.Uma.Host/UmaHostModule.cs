using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdServer.Module;
using SimpleIdServer.Uma.Host.Controllers;
using SimpleIdServer.Uma.Host.Extensions;

namespace SimpleIdServer.Uma.Host
{
    public class UmaHostModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            services.AddUmaHost(new AuthorizationServerOptions());
        }
		
        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            var mvcBuilder = AspPipelineContext.Instance().ConfigureServiceContext.MvcBuilder;
            var assembly = typeof(JwksController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
        }

        private void HandleAuthorizationAdded(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddUmaSecurityPolicy();
        }
    }
}