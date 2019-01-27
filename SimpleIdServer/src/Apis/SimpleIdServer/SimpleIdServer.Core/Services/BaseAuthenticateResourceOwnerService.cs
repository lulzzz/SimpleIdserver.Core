using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public abstract class BaseAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        public abstract string Amr { get; }

        private readonly ICredentialSettingsRepository _passwordSettingsRepository;
        protected readonly IResourceOwnerRepository ResourceOwnerRepository;

        public BaseAuthenticateResourceOwnerService(ICredentialSettingsRepository passwordSettingsRepository, IResourceOwnerRepository resourceOwnerRepository)
        {
            _passwordSettingsRepository = passwordSettingsRepository;
            ResourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string credentialValue)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(credentialValue))
            {
                throw new ArgumentNullException(nameof(credentialValue));
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

            var passwordSettigns = await _passwordSettingsRepository.Get(Amr).ConfigureAwait(false);
            var currentDateTime = DateTime.UtcNow;
            var minCurrentEndDate = currentDateTime.AddSeconds(-passwordSettigns.AuthenticationIntervalsInSeconds);
            var credential = resourceOwner.Credentials.First(c => c.Type == Amr);
            if (credential.FirstAuthenticationFailureDateTime != null && credential.FirstAuthenticationFailureDateTime.Value.AddSeconds(passwordSettigns.AuthenticationIntervalsInSeconds) >= minCurrentEndDate && credential.NumberOfAttempts >= passwordSettigns.NumberOfAuthenticationAttempts)
            {
                throw new IdentityServerUserTooManyRetryException
                {
                    RetryInSeconds = passwordSettigns.AuthenticationIntervalsInSeconds
                };
            }

            if (!await Authenticate(resourceOwner, credentialValue).ConfigureAwait(false))
            {
                if (passwordSettigns.IsBlockAccountPolicyEnabled)
                {
                    if (credential.FirstAuthenticationFailureDateTime == null || credential.FirstAuthenticationFailureDateTime.Value.AddSeconds(passwordSettigns.AuthenticationIntervalsInSeconds) < minCurrentEndDate)
                    {
                        credential.NumberOfAttempts = 1;
                        credential.FirstAuthenticationFailureDateTime = currentDateTime;
                    }
                    else
                    {
                        credential.NumberOfAttempts++;
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
