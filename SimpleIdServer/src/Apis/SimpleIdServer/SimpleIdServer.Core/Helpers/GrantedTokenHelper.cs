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

using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.Store;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Helpers
{
    public interface IGrantedTokenHelper 
    {
        Task<GrantedToken> GetValidGrantedTokenAsync(string scopes, string clientId, JwsPayload idTokenJwsPayload = null, JwsPayload userInfoJwsPayload = null);
    }

    internal class GrantedTokenHelper : IGrantedTokenHelper
    {
        private readonly ITokenStore _tokenStore;
        private readonly IGrantedTokenValidator _grantedTokenValidator;
        
        public GrantedTokenHelper(ITokenStore tokenStore, IGrantedTokenValidator grantedTokenValidator)
        {
            _tokenStore = tokenStore;
            _grantedTokenValidator = grantedTokenValidator;
        }

        public async Task<GrantedToken> GetValidGrantedTokenAsync(string scopes, string clientId, JwsPayload idTokenJwsPayload = null, JwsPayload userInfoJwsPayload = null)
        {
            if (string.IsNullOrWhiteSpace(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var token = await _tokenStore.GetToken(scopes, clientId, idTokenJwsPayload, userInfoJwsPayload);
            if (token == null)
            {
                return null;
            }

            if (!_grantedTokenValidator.CheckGrantedToken(token).IsValid)
            {
                await _tokenStore.RemoveAccessToken(token.AccessToken);
                return null;
            }

            return token;
        }
    }
}
