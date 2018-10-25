using SimpleIdServer.Dtos.Responses;

namespace SimpleIdServer.Client.Results
{
    public class GetRegisterClientResult : BaseSidResult
    {
        public ClientRegistrationResponse Content { get; set; }
    }
}
