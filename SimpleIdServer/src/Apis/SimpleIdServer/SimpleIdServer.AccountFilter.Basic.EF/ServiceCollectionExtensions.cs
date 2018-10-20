using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.AccountFilter.Basic.EF.Repositories;
using SimpleIdServer.AccountFilter.Basic.Repositories;

namespace SimpleIdServer.AccountFilter.Basic.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilterRepositories(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IFilterRepository, FilterRepository>();
            return serviceCollection;
        }
    }
}
