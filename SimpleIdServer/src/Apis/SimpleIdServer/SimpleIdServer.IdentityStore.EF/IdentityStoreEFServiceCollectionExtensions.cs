using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.IdentityStore.EF.Repositories;
using SimpleIdServer.IdentityStore.Repositories;

namespace SimpleIdServer.IdentityStore.EF
{
    public static class IdentityStoreEFServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityStoreEF(this IServiceCollection services)
        {
            services.AddTransient<ICredentialSettingsRepository, CredentialSettingsRepository>();
            services.AddTransient<IUserCredentialRepository, UserCredentialRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            return services;
        }
    }
}
