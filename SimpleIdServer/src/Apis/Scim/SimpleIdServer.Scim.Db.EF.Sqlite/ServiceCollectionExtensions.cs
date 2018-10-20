using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdServer.Scim.Db.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimSqliteEF(this IServiceCollection serviceCollection, string connectionString, Action<SqliteDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

			serviceCollection.AddScimRepository();
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<ScimDbContext>(options =>
                    options.UseSqlite(connectionString, callback));
            return serviceCollection;
        }
    }
}
