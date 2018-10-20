﻿using Newtonsoft.Json;
using SimpleIdServer.Common.Client;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.UserManagement.Common.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdServer.UserManagement.Client.Operations
{
    public interface ILinkProfileOperation
    {
	    Task<BaseResponse> Execute(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> Execute(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
    }

    internal sealed class LinkProfileOperation : ILinkProfileOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LinkProfileOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<BaseResponse> Execute(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if(linkProfileRequest == null)
            {
                throw new ArgumentNullException(nameof(linkProfileRequest));
            }

            var url = requestUrl + "/.me";
            return Link(url, linkProfileRequest, authorizationHeaderValue);
        }

        public Task<BaseResponse> Execute(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if(linkProfileRequest == null)
            {
                throw new ArgumentNullException(nameof(linkProfileRequest));
            }


            var url = requestUrl + $"/{currentSubject}";
            return Link(url, linkProfileRequest, authorizationHeaderValue);
        }

        private async Task<BaseResponse> Link(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(linkProfileRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(requestUrl)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new BaseResponse();
        }
    }
}
