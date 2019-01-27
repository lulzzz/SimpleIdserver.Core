using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Repositories;

namespace SimpleIdServer.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthRepositories(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<ITranslationRepository, TranslationRepository>();
            serviceCollection.AddTransient<IResourceOwnerRepository, ResourceOwnerRepository>();
            serviceCollection.AddTransient<IScopeRepository, ScopeRepository>();
            serviceCollection.AddTransient<IClientRepository, ClientRepository>();
            serviceCollection.AddTransient<IConsentRepository, ConsentRepository>();
            serviceCollection.AddTransient<IJsonWebKeyRepository, JsonWebKeyRepository>();
            serviceCollection.AddTransient<IClaimRepository, ClaimRepository>();
            serviceCollection.AddTransient<IProfileRepository, ProfileRepository>();
            serviceCollection.AddTransient<ICredentialSettingsRepository, PasswordSettingsRepository>();
            return serviceCollection;
        }
    }
}
