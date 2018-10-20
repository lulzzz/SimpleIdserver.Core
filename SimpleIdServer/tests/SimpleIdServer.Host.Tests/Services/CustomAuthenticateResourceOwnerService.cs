﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Services;

namespace SimpleIdServer.Host.Tests.Services
{
    public class CustomAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public CustomAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public string Amr => "pwd";

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

            return await _resourceOwnerRepository.GetAsync(login, GetHashedPassword(password));
        }

        public string GetHashedPassword(string password)
        {
            return password;
        }
    }
}
