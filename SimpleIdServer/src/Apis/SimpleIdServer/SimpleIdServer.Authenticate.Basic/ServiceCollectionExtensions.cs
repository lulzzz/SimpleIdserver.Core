using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Authenticate.Basic.Actions;
using SimpleIdServer.Authenticate.Basic.Helpers;

namespace SimpleIdServer.Authenticate.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthBasic(this IServiceCollection services)
        {
            services.AddTransient<IOpenidAuthenticateResourceOwnerAction, OpenidAuthenticateResourceOwnerAction>();
            services.AddTransient<IAuthenticateHelper, AuthenticateHelper>();
            return services;
        }
    }
}
