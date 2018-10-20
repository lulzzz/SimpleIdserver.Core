using SimpleIdServer.Core.Common.DTOs.Responses;

namespace SimpleIdServer.Client.Results
{
    public class GetTokenResult : BaseSidResult
    {
        public GrantedTokenResponse Content { get; set; }
    }
}
