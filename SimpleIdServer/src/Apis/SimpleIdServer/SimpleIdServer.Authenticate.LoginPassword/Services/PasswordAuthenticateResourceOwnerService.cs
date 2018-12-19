using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Services
{
    internal sealed class PasswordAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        public PasswordAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, IPasswordSettingsRepository passwordSettingsRepository) : base(passwordSettingsRepository, resourceOwnerRepository)
        {
        }

        public override string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public override Task<bool> Authenticate(ResourceOwner user, string password)
        {
            return Task.FromResult(user.Password == PasswordHelper.ComputeHash(password));
        }

        public override Task<ResourceOwner> GetResourceOwner(string login)
        {
            return ResourceOwnerRepository.GetAsync(login);
        }
    }
}