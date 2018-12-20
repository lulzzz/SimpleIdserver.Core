using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public abstract class BaseAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        public abstract string Amr { get; }

        private readonly IPasswordSettingsRepository _passwordSettingsRepository;
        protected readonly IResourceOwnerRepository ResourceOwnerRepository;

        public BaseAuthenticateResourceOwnerService(IPasswordSettingsRepository passwordSettingsRepository, IResourceOwnerRepository resourceOwnerRepository)
        {
            _passwordSettingsRepository = passwordSettingsRepository;
            ResourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var resourceOwner = await GetResourceOwner(login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerUserAccountDoesntExistException();
            }
            
            if (resourceOwner.IsBlocked)
            {
                throw new IdentityServerUserAccountBlockedException();
            }

            var passwordSettigns = await _passwordSettingsRepository.Get().ConfigureAwait(false);
            var currentDateTime = DateTime.UtcNow;
            var minCurrentEndDate = currentDateTime.AddSeconds(-passwordSettigns.AuthenticationIntervalsInSeconds);
            if (resourceOwner.FirstAuthenticationFailureDateTime != null && resourceOwner.FirstAuthenticationFailureDateTime.Value.AddSeconds(passwordSettigns.AuthenticationIntervalsInSeconds) >= minCurrentEndDate && resourceOwner.NumberOfAttempts >= passwordSettigns.NumberOfAuthenticationAttempts)
            {
                throw new IdentityServerUserTooManyRetryException
                {
                    RetryInSeconds = passwordSettigns.AuthenticationIntervalsInSeconds
                };
            }

            if (!await Authenticate(resourceOwner, password).ConfigureAwait(false))
            {
                if (passwordSettigns.IsBlockAccountPolicyEnabled)
                {
                    if (resourceOwner.FirstAuthenticationFailureDateTime == null || resourceOwner.FirstAuthenticationFailureDateTime.Value.AddSeconds(passwordSettigns.AuthenticationIntervalsInSeconds) < minCurrentEndDate)
                    {
                        resourceOwner.NumberOfAttempts = 1;
                        resourceOwner.FirstAuthenticationFailureDateTime = currentDateTime;
                    }
                    else
                    {
                        resourceOwner.NumberOfAttempts++;
                    }
                    
                    await ResourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
                }

                throw new IdentityServerUserPasswordInvalidException();
            }

            await Validate(resourceOwner).ConfigureAwait(false);
            return resourceOwner;
        }

        public abstract Task<ResourceOwner> GetResourceOwner(string login);
        public abstract Task<bool> Authenticate(ResourceOwner user, string password);
        public abstract Task Validate(ResourceOwner user);
    }
}
