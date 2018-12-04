using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.EF.Repositories;
using System;

namespace SimpleIdServer.Uma.EF
{
    public static class SimpleIdServerUmaExtensions
    {        
        public static IServiceCollection AddUmaRepositories(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IResourceSetRepository, ResourceSetRepository>();
            serviceCollection.AddTransient<IPolicyRepository, PolicyRepository>();
            serviceCollection.AddTransient<ISharedLinkRepository, SharedLinkRepository>();
            serviceCollection.AddTransient<IPendingRequestRepository, PendingRequestRepository>();
            return serviceCollection;
        }
    }
}
