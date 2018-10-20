using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdServer.AccountFilter.Basic.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicAccountFilterSqlServerEF(this IServiceCollection serviceCollection, string connectionString, Action<SqlServerDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddAccountFilterRepositories();
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<AccountFilterBasicServerContext>(options =>
                    options.UseSqlServer(connectionString, callback));
            return serviceCollection;
        }
    }
}
