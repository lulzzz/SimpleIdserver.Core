using SimpleIdServer.Core.Common.DTOs.Responses;

namespace SimpleIdServer.Client.Results
{
    public class GetRegisterClientResult : BaseSidResult
    {
        public ClientRegistrationResponse Content { get; set; }
    }
}
