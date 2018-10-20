﻿#region copyright
// Copyright 2016 Habart Thierry
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
using SimpleIdServer.Core.Common.DTOs.Requests;

namespace SimpleIdServer.Client
{
    public interface IAuthorizationClient
    {
        Task<GetAuthorizationResult> ExecuteAsync(string authorizationUrl, AuthorizationRequest request);
        Task<GetAuthorizationResult> ExecuteAsync(Uri authorizationUri, AuthorizationRequest request);
        Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request);
    }

    internal class AuthorizationClient : IAuthorizationClient
    {
        private readonly IGetAuthorizationOperation _getAuthorizationOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public AuthorizationClient(IGetAuthorizationOperation getAuthorizationOperation, IGetDiscoveryOperation getDiscoveryOperation)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public Task<GetAuthorizationResult> ExecuteAsync(Uri authorizationUri, AuthorizationRequest request)
        {
            if (authorizationUri == null)
            {
                throw new ArgumentNullException(nameof(authorizationUri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _getAuthorizationOperation.ExecuteAsync(authorizationUri, request);
        }

        public Task<GetAuthorizationResult> ExecuteAsync(string authorizationUrl, AuthorizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(authorizationUrl))
            {
                throw new ArgumentNullException(nameof(authorizationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(authorizationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, authorizationUrl));
            }

            return ExecuteAsync(uri, request);
        }

        public async Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(discoveryDocumentationUrl))
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(discoveryDocumentationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);
            return await ExecuteAsync(discoveryDocument.AuthorizationEndPoint, request).ConfigureAwait(false);
        }
    }
}
