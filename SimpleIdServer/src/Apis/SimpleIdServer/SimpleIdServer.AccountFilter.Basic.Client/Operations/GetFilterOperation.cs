using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleIdServer.AccountFilter.Basic.Client.Results;
using SimpleIdServer.AccountFilter.Basic.Common.Responses;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Common.Dtos.Responses;

namespace SimpleIdServer.AccountFilter.Basic.Client.Operations
{
    public interface IGetFilterOperation
    {
        Task<GetFilterResult> Execute(string requestUrl, string filterId, string authorizationHeaderValue = null);
    }

    internal sealed class GetFilterOperation : IGetFilterOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetFilterOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetFilterResult> Execute(string requestUrl, string filterId, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(filterId))
            {
                throw new ArgumentNullException(nameof(filterId));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUrl + $"/{filterId}")
            };
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
                return new GetFilterResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GetFilterResult
            {
                Content = JsonConvert.DeserializeObject<FilterResponse>(content),
                HttpStatus = result.StatusCode
            };
        }
    }
}
