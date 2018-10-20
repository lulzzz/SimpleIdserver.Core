using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Scim.Core.Stores;
using SimpleIdServer.Scim.Db.EF.Stores;

namespace SimpleIdServer.Scim.Db.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISchemaStore, SchemaStore>();
            serviceCollection.AddTransient<IRepresentationStore, RepresentationStore>();
            return serviceCollection;
        }
    }
}
