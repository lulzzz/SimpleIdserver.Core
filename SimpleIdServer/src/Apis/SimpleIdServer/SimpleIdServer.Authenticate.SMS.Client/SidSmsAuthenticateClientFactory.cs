using SimpleIdServer.Common.Client.Factories;

namespace SimpleIdServer.Authenticate.SMS.Client
{
    public interface ISidSmsAuthenticateClientFactory
    {
        ISidSmsAuthenticateClient GetClient();
    }

    public class SidSmsAuthenticateClientFactory : ISidSmsAuthenticateClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SidSmsAuthenticateClientFactory()
        {
            _httpClientFactory = new HttpClientFactory();
        }

        public SidSmsAuthenticateClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ISidSmsAuthenticateClient GetClient()
        {
            return new SidSmsAuthenticateClient(new SendSmsOperation(_httpClientFactory));
        }
    }
}