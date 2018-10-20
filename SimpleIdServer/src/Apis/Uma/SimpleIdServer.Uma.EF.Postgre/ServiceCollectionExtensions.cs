﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace SimpleIdServer.Uma.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaPostgreEF(this IServiceCollection serviceCollection, string connectionString, Action<NpgsqlDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddUmaRepositories();
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<SimpleIdServerUmaContext>(options =>
                    options.UseNpgsql(connectionString, callback));
            return serviceCollection;
        }
    }
}
