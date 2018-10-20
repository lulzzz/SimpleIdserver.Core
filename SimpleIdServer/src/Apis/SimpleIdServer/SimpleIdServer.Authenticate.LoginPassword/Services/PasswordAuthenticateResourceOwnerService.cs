using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;

namespace SimpleIdServer.Authenticate.LoginPassword.Services
{
    internal sealed class PasswordAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public PasswordAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            return _resourceOwnerRepository.GetAsync(login, PasswordHelper.ComputeHash(password));
        }
    }
}