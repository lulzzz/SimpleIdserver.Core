using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System.Collections.Generic;

namespace SimpleIdServer.IdentityStore
{
    public static class IdentityStoreEFServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityStoreEF(this IServiceCollection services, ICollection<User> users, IEnumerable<CredentialSetting> credentialSettings = null)
        {
            services.AddSingleton<ICredentialSettingsRepository>(new DefaultCredentialSettingRepository(credentialSettings));
            services.AddSingleton<IUserRepository>(new DefaultUserRepository(users));
            services.AddSingleton<IUserCredentialRepository>(new DefaultUserCredentialRepository(users));
            return services;
        }
    }
}