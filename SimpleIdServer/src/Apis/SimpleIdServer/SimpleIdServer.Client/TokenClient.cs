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
using SimpleIdServer.Client.Builders;
using SimpleIdServer.Client.Errors;
using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Results;

namespace SimpleIdServer.Client
{
    public interface ITokenClient
    {
        Task<GetTokenResult> ExecuteAsync(string tokenUrl);
        Task<GetTokenResult> ExecuteAsync(Uri tokenUri);
        Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }

    internal class TokenClient : ITokenClient
    {
        private readonly IPostTokenOperation _postTokenOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;
        private readonly RequestBuilder _requestBuilder;
        
        public TokenClient(
            RequestBuilder requestBuilder,
            IPostTokenOperation postTokenOperation,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _requestBuilder = requestBuilder;
            _postTokenOperation = postTokenOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }
        
        public Task<GetTokenResult> ExecuteAsync(string tokenUrl)
        {
            if (string.IsNullOrWhiteSpace(tokenUrl))
            {
                throw new ArgumentNullException(nameof(tokenUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(tokenUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, tokenUrl));
            }

            return ExecuteAsync(uri);
        }

        public Task<GetTokenResult> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }
            
            if (_requestBuilder.Certificate != null)
            {
                return _postTokenOperation.ExecuteAsync(_requestBuilder.Content,
                    tokenUri,
                    _requestBuilder.AuthorizationHeaderValue,
                    _requestBuilder.Certificate);
            }
            
            return _postTokenOperation.ExecuteAsync(_requestBuilder.Content,
                tokenUri,
                _requestBuilder.AuthorizationHeaderValue);
        }

        public async Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl)
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
            return await ExecuteAsync(discoveryDocument.TokenEndPoint).ConfigureAwait(false);
        }
    }
}
