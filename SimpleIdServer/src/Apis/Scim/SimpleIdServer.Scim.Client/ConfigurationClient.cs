﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Common.Client.Factories;

namespace SimpleIdServer.Scim.Client
{
    public interface IConfigurationClient
    {
        Task<JObject> GetServiceProviderConfig(string baseUri);
        Task<JArray> GetSchemas(string baseUri);
    }

    internal class ConfigurationClient : IConfigurationClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfigurationClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JObject> GetServiceProviderConfig(string baseUri)
        {
            return JObject.Parse(await Get(new Uri($"{baseUri}/ServiceProviderConfig")).ConfigureAwait(false));
        }

        public async Task<JArray> GetSchemas(string baseUri)
        {
            return JArray.Parse(await Get(new Uri($"{baseUri}/Schemas")).ConfigureAwait(false));
        }

        private async Task<string> Get(Uri baseUri)
        {
            var client = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = baseUri
            };
            var result = await client.SendAsync(request).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
