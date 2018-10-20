using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Common.Client;

namespace SimpleIdServer.Authenticate.SMS.Client
{
    public interface ISidSmsAuthenticateClientFactory
    {
        ISidSmsAuthenticateClient GetClient();
    }

    public class SidSmsAuthenticateClientFactory : ISidSmsAuthenticateClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SidSmsAuthenticateClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public ISidSmsAuthenticateClient GetClient()
        {
            var result = _serviceProvider.GetService<ISidSmsAuthenticateClient>();
            return result;
        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ISidSmsAuthenticateClient, SidSmsAuthenticateClient>();
            services.AddTransient<ISendSmsOperation, SendSmsOperation>();
            services.AddCommonClient();
        }
    }
}