using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;
using SimpleIdServer.IdentityStore;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Host.Tests.Services
{
    public class CustomAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        private readonly IUserRepository _resourceOwnerRepository;

        public CustomAuthenticateResourceOwnerService(IUserRepository resourceOwnerRepository, ICredentialSettingsRepository passwordSettingsRepository,
            IUserCredentialRepository resourceOwnerCredentialRepository) : base(passwordSettingsRepository, resourceOwnerCredentialRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public override string Amr
        {
            get
            {
                return "pwd";
            }
        }

        public override Task<bool> Authenticate(User user, string credentialValue)
        {
            var credential = user.Credentials.FirstOrDefault(c => c.Type == "pwd");
            if (credential == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(credential.Value == PasswordHelper.ComputeHash(credentialValue));
        }

        public override Task<User> GetUser(string login)
        {
            return _resourceOwnerRepository.Get(login);
        }

        public override Task Validate(User user)
        {
            var credential = user.Credentials.FirstOrDefault(c => c.Type == "pwd");
            if (credential == null)
            {
                return Task.FromResult(false);
            }

            if (credential.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new IdentityServerPasswordExpiredException(user);
            }

            return Task.FromResult(0);
        }
    }
}
