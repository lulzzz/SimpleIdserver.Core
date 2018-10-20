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
using SimpleIdServer.Client.Errors;
using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Results;

namespace SimpleIdServer.Client
{
    public interface IUserInfoClient
    {
        GetUserInfoResult GetUserInfo(string userInfoUrl, string accessToken, bool inBody = false);
        GetUserInfoResult GetUserInfo(Uri userInfoUri, string accessToken, bool inBody = false);
        Task<GetUserInfoResult> GetUserInfoAsync(string userInfoUrl, string accessToken, bool inBody = false);
        Task<GetUserInfoResult> GetUserInfoAsync(Uri userInfoUri, string accessToken, bool inBody = false);
        Task<GetUserInfoResult> Resolve(string configurationUrl, string accessToken, bool inBody = false);
    }

    internal class UserInfoClient : IUserInfoClient
    {
        private readonly IGetUserInfoOperation _getUserInfoOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public UserInfoClient(IGetUserInfoOperation getUserInfoOperation,
            IGetDiscoveryOperation getDiscoveryOperation, bool inBody = false)
        {
            if (getUserInfoOperation == null)
            {
                throw new ArgumentNullException(nameof(getUserInfoOperation));
            }

            if (getDiscoveryOperation == null)
            {
                throw new ArgumentNullException(nameof(getDiscoveryOperation));
            }

            _getUserInfoOperation = getUserInfoOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public GetUserInfoResult GetUserInfo(Uri userInfoUri, string authorizationHeader, bool inBody = false)
        {
            return GetUserInfoAsync(userInfoUri, authorizationHeader).Result;
        }

        public GetUserInfoResult GetUserInfo(string userInfoUrl, string accessToken, bool inBody = false)
        {
            return GetUserInfoAsync(userInfoUrl, accessToken).Result;
        }

        public Task<GetUserInfoResult> GetUserInfoAsync(string userInfoUrl, string accessToken, bool inBody = false)
        {
            if (string.IsNullOrWhiteSpace(userInfoUrl))
            {
                throw new ArgumentNullException(nameof(userInfoUrl));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            Uri uri = null;
            if (!Uri.TryCreate(userInfoUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, userInfoUrl));
            }

            return GetUserInfoAsync(uri, accessToken);
        }

        public Task<GetUserInfoResult> GetUserInfoAsync(Uri userInfoUri, string accessToken, bool inBody = false)
        {
            if (userInfoUri == null)
            {
                throw new ArgumentNullException(nameof(userInfoUri));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            
            return _getUserInfoOperation.ExecuteAsync(userInfoUri, accessToken, inBody);
        }

        public async Task<GetUserInfoResult> Resolve(string configurationUrl, string accessToken, bool inBody = false)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            Uri uri = null;
            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);
            return await GetUserInfoAsync(discoveryDocument.UserInfoEndPoint, accessToken).ConfigureAwait(false);
        }
    }
}
