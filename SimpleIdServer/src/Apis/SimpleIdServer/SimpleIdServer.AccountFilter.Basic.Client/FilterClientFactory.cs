using SimpleIdServer.AccountFilter.Basic.Client.Operations;
using SimpleIdServer.Common.Client.Factories;

namespace SimpleIdServer.AccountFilter.Basic.Client
{
    public interface IFilterClientFactory
    {
        IFilterClient GetFilterClient();
    }

    public class FilterClientFactory : IFilterClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FilterClientFactory()
        {
            _httpClientFactory = new HttpClientFactory();
        }

        public FilterClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IFilterClient GetFilterClient()
        {
            return new FilterClient(new AddFilterOperation(_httpClientFactory), new DeleteFilterOperation(_httpClientFactory), new GetAllFiltersOperation(_httpClientFactory), new UpdateFilterOperation(_httpClientFactory),
                new GetFilterOperation(_httpClientFactory));
        }
    }
}
