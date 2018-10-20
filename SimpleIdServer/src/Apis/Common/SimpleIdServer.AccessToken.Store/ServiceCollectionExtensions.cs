using System;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdServer.AccessToken.Store
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultAccessTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IAccessTokenStore, AccessTokenStore>();
            return serviceCollection;
        }
    }
}
