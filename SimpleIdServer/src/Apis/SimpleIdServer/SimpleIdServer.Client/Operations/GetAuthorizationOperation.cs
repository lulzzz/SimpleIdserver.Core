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
using Newtonsoft.Json;
using SimpleIdServer.Client.Results;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Dtos.Responses;
using SimpleIdServer.Lib;

namespace SimpleIdServer.Client.Operations
{
    public interface IGetAuthorizationOperation
    {
        Task<GetAuthorizationResult> ExecuteAsync(Uri uri, AuthorizationRequest request);
    }

    internal class GetAuthorizationOperation : IGetAuthorizationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAuthorizationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAuthorizationResult> ExecuteAsync(Uri uri, AuthorizationRequest request)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var uriBuilder = new UriBuilder(uri);
            var pSerializer = new ParamSerializer();            
            uriBuilder.Query = pSerializer.Serialize(request);
            var response = await httpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode >= System.Net.HttpStatusCode.BadRequest)
            {
                return new GetAuthorizationResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                    Status = response.StatusCode
                };
            }

            return new GetAuthorizationResult
            {
                ContainsError = false,
                Location = response.Headers.Location
            };
        }
    }
}