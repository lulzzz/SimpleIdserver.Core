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

using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IAddResourceToPolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, PostAddResourceSet request, string url, string token);
    }

    internal class AddResourceToPolicyOperation : IAddResourceToPolicyOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddResourceToPolicyOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(string id, PostAddResourceSet request, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id + "/resources";
            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add("Authorization", "Bearer " + token);
            var httpResult = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }
    }
}
