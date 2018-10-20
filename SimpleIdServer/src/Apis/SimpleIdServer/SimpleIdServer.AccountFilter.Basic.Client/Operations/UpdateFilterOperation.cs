using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleIdServer.AccountFilter.Basic.Common.Requests;
using SimpleIdServer.Common.Client;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Common.Dtos.Responses;

namespace SimpleIdServer.AccountFilter.Basic.Client.Operations
{
    public interface IUpdateFilterOperation
    {
        Task<BaseResponse> Execute(string requestUrl, UpdateFilterRequest updateFilterRequest, string authorizationHeaderValue = null);
    }

    internal sealed class UpdateFilterOperation : IUpdateFilterOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateFilterOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> Execute(string requestUrl, UpdateFilterRequest updateFilterRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (updateFilterRequest == null)
            {
                throw new ArgumentNullException(nameof(updateFilterRequest));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(updateFilterRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
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

            return new BaseResponse
            {
                HttpStatus = result.StatusCode
            };
        }
    }
}
