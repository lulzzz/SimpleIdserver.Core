using SimpleIdServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdServer.UserManagement
{
    public class UserManagementModule : IModule
    {
        private IDictionary<string, string> _options;

        public void Init(IDictionary<string, string> options)
        {
            _options = options;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleConfigureRoute;
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddUserManagement(configureServiceContext.MvcBuilder, GetOptions(_options));
        }

        private void HandleConfigureRoute(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteBuilder.UseUserManagement();
        }

        private static UserManagementOptions GetOptions(IDictionary<string, string> options)
        {
            var result = new UserManagementOptions();
            if (options != null)
            {
                bool canUpdateTwoFactorAuthentication;
                if (options.TryGetValue("CanUpdateTwoFactorAuthentication", out canUpdateTwoFactorAuthentication))
                {
                    result.CanUpdateTwoFactorAuthentication = canUpdateTwoFactorAuthentication;
                }
            }

            return result;
        }
    }
}