using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Uma.Core.Policies;

namespace SimpleIdServer.Uma.Core.Responses
{
    public class GetTokenByTicketIdResponse
    {
        public bool IsValid { get; set; }
        public GrantedToken GrantedToken { get; set; }
        public ResourceValidationResult ResourceValidationResult { get; set; }
    }
}
