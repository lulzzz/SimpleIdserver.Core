using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Common.Client;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.UserManagement.Client.Operations;

namespace SimpleIdServer.UserManagement.Client
{
    public interface IProfileClientFactory
    {
        IProfileClient GetProfileClient();
    }

    public class ProfileClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ProfileClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public ProfileClientFactory(IHttpClientFactory httpClientFactory)
        {
            var services = new ServiceCollection();
            RegisterDependencies(services, httpClientFactory);
            _serviceProvider = services.BuildServiceProvider();
        }

        public IProfileClient GetProfileClient()
        {
            var result = _serviceProvider.GetService<IProfileClient>();
            return result;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection, IHttpClientFactory httpClientFactory = null)
        {
            if (httpClientFactory != null)
            {
                serviceCollection.AddSingleton(httpClientFactory);
            }
            else
            {
                serviceCollection.AddCommonClient();
            }

            serviceCollection.AddTransient<IUnlinkProfileOperation, UnlinkProfileOperation>();
            serviceCollection.AddTransient<ILinkProfileOperation, LinkProfileOperation>();
            serviceCollection.AddTransient<IGetProfilesOperation, GetProfilesOperation>();
            serviceCollection.AddTransient<IProfileClient, ProfileClient>();
        }
    }
}
