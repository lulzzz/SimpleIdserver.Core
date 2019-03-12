using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Services
{
    internal sealed class PasswordAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        private readonly IUserRepository _userRepository;

        public PasswordAuthenticateResourceOwnerService(ICredentialSettingsRepository credentialSettingsRepository, IUserCredentialRepository userCredentialRepository,
            IUserRepository userRepository) : base(credentialSettingsRepository, userCredentialRepository)
        {
            _userRepository = userRepository;
        }

        public override string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public override Task<bool> Authenticate(User user, string credentialValue)
        {
            return _userRepository.Authenticate(user.Id, credentialValue);
        }

        public override Task<User> GetUser(string login)
        {
            return _userRepository.Get(login);
        }

        public override Task Validate(User user)
        {
            var credential = user.Credentials.FirstOrDefault(c => c.Type == Constants.AMR);
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