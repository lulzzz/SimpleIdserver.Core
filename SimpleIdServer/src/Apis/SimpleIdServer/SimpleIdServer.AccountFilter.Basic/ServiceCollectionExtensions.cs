using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.AccountFilter.Basic.Aggregates;
using SimpleIdServer.AccountFilter.Basic.Controllers;
using SimpleIdServer.AccountFilter.Basic.Repositories;

namespace SimpleIdServer.AccountFilter.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilter(this IServiceCollection services, IMvcBuilder mvcBuilder, List<FilterAggregate> filters = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            var assembly = typeof(FiltersController).Assembly;
            mvcBuilder.AddApplicationPart(assembly);
            services.AddTransient<IAccountFilter, AccountFilter>();
            services.AddSingleton<IFilterRepository>(new DefaultFilterRepository(filters));
            return services;
        }
    }
}
