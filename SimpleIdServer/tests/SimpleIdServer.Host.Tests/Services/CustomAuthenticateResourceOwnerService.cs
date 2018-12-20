﻿using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Services;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Host.Tests.Services
{
    public class CustomAuthenticateResourceOwnerService : BaseAuthenticateResourceOwnerService
    {
        public CustomAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, IPasswordSettingsRepository passwordSettingsRepository) : base(passwordSettingsRepository, resourceOwnerRepository)
        {
        }

        public override string Amr
        {
            get
            {
                return "pwd";
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

        public override Task Validate(ResourceOwner user)
        {
            if (user.PasswordExpirationDateTime < DateTime.UtcNow)
            {
                throw new IdentityServerPasswordExpiredException(user);
            }

            return Task.FromResult(0);
        }
    }
}
