using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.IdentityStore.LDAP.Repositories;
using SimpleIdServer.IdentityStore.Repositories;
using System;

namespace SimpleIdServer.IdentityStore.LDAP
{
    public static class IdentityStoreLDAPServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityStoreLDAP(this IServiceCollection services, IdentityStoreLDAPOptions identityStoreLDAPOptions)
        {
            if (identityStoreLDAPOptions == null)
            {
                throw new ArgumentNullException(nameof(identityStoreLDAPOptions));
            }

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddSingleton(identityStoreLDAPOptions);
            return services;
        }
    }
}
