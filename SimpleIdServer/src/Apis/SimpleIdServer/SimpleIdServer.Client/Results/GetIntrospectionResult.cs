using SimpleIdServer.Dtos.Responses;

namespace SimpleIdServer.Client.Results
{
    public class GetIntrospectionResult : BaseSidResult
    {
        public IntrospectionResponse Content { get; set; }
    }
}
