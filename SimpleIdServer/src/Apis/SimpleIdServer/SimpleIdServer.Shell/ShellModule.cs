using System;
using System.Collections.Generic;
using SimpleIdServer.Module;

namespace SimpleIdServer.Shell
{
    public class ShellModule : IModule
    {
        private IDictionary<string, string> _options;

        public void Init(IDictionary<string, string> options)
        {
            _options = options;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleRouteConfigured;
        }

        private void HandleApplicationBuilderInitialized(object sender, EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.App.UseShellStaticFiles();
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddBasicShell(configureServiceContext.MvcBuilder, GetOptions());
        }

        private void HandleRouteConfigured(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteBuilder.UseShell();
        }

        private ShellModuleOptions GetOptions()
        {
            var result = new ShellModuleOptions();
            if (_options != null)
            {
                string clientId;
                string clientSecret;
                if (_options.TryGetValue("ClientId", out clientId))
                {
                    result.ClientId = clientId;
                }

                if (_options.TryGetValue("ClientSecret", out clientSecret))
                {
                    result.ClientSecret = clientSecret;
                }
            }

            return result;
        }
    }
}
