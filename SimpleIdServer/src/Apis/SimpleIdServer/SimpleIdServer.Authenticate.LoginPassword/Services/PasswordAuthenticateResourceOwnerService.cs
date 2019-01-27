﻿using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Services
{
    internal sealed class PasswordAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        public PasswordAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, ICredentialSettingsRepository passwordSettingsRepository) : base(passwordSettingsRepository, resourceOwnerRepository)
        {
        }

        public override string Amr
        {
            get
            {
                return Constants.AMR;
            }
        }

        public override Task<bool> Authenticate(ResourceOwner user, string credentialValue)
        {
            var credential = user.Credentials.FirstOrDefault(c => c.Type == Constants.AMR);
            if (credential == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(credential.Value == PasswordHelper.ComputeHash(credentialValue));
        }

        public override Task<ResourceOwner> GetResourceOwner(string login)
        {
            return ResourceOwnerRepository.GetAsync(login);
        }

        public override Task Validate(ResourceOwner user)
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