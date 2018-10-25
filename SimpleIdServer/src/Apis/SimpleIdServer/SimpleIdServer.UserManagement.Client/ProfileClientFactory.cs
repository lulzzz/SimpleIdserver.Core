using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.UserManagement.Client.Operations;

namespace SimpleIdServer.UserManagement.Client
{
    public interface IProfileClientFactory
    {
        IProfileClient GetProfileClient();
    }

    public class ProfileClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileClientFactory()
        {
            _httpClientFactory = new HttpClientFactory();
        }

        public ProfileClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IProfileClient GetProfileClient()
        {
            return new ProfileClient(new LinkProfileOperation(_httpClientFactory), new UnlinkProfileOperation(_httpClientFactory), new GetProfilesOperation(_httpClientFactory));
        }
    }
}
