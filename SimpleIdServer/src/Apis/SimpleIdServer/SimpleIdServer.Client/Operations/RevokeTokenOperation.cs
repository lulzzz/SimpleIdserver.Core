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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleIdServer.Client.Results;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Dtos.Responses;

namespace SimpleIdServer.Client.Operations
{
    public interface IRevokeTokenOperation
    {
        Task<GetRevokeTokenResult> ExecuteAsync(Dictionary<string, string> revokeParameter, Uri requestUri, string authorizationValue);
    }

    internal class RevokeTokenOperation : IRevokeTokenOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RevokeTokenOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetRevokeTokenResult> ExecuteAsync(Dictionary<string, string> revokeParameter, Uri requestUri, string authorizationValue)
        {
            if (revokeParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeParameter));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var body = new FormUrlEncodedContent(revokeParameter);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Basic " + authorizationValue);
            }

            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                return new GetRevokeTokenResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json),
                    Status = result.StatusCode
                };
            }

            return new GetRevokeTokenResult();
        }
    }
}
