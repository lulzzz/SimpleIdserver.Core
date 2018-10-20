using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Uma.Client.Results;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Policy
{
    public interface ISearchPoliciesOperation
    {
        Task<SearchAuthPoliciesResult> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }

    internal sealed class SearchPoliciesOperation : ISearchPoliciesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchPoliciesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchAuthPoliciesResult> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await httpClient.SendAsync(request);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new SearchAuthPoliciesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new SearchAuthPoliciesResult
            {
                Content = JsonConvert.DeserializeObject<SearchAuthPoliciesResponse>(content)
            };
        }
    }
}
