using SimpleIdServer.Uma.Core.Api.Token.Actions;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Responses;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.Token
{
    public interface IUmaTokenActions
    {
        Task<GetTokenByTicketIdResponse> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, string openidProvider, string issuerName);
    }

    internal sealed class UmaTokenActions : IUmaTokenActions
    {
        private readonly IGetTokenByTicketIdAction _getTokenByTicketIdAction;

        public UmaTokenActions(IGetTokenByTicketIdAction getTokenByTicketIdAction)
        {
            _getTokenByTicketIdAction = getTokenByTicketIdAction;
        }

        public Task<GetTokenByTicketIdResponse> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, string openidProvider, string issuerName)
        {
            return _getTokenByTicketIdAction.Execute(parameter, openidProvider, issuerName);
        }
    }
}
