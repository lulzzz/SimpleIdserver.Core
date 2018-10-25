using SimpleIdServer.Dtos.Responses;

namespace SimpleIdServer.Client.Results
{
    public class GetTokenResult : BaseSidResult
    {
        public GrantedTokenResponse Content { get; set; }
    }
}
