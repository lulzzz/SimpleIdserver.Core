using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Core.Authenticate;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Store;
using SimpleIdServer.Uma.Common;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Policies;
using SimpleIdServer.Uma.Core.Responses;
using SimpleIdServer.Uma.Core.Services;
using SimpleIdServer.Uma.Core.Stores;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.Token.Actions
{
    public interface IGetTokenByTicketIdAction
    {
        Task<GetTokenByTicketIdResponse> Execute(GetTokenViaTicketIdParameter parameter, string openidProvider, string issuerName);
    }

    internal sealed class GetTokenByTicketIdAction : IGetTokenByTicketIdAction
    {
        private readonly ITicketStore _ticketStore;
        private readonly IUmaConfigurationService _configurationService;
        private readonly IUmaServerEventSource _umaServerEventSource;
        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;

        public GetTokenByTicketIdAction(ITicketStore ticketStore, IUmaConfigurationService configurationService,
            IUmaServerEventSource umaServerEventSource, IAuthorizationPolicyValidator authorizationPolicyValidator, IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient, IJwtGenerator jwtGenerator, IClientHelper clientHelper, ITokenStore tokenStore)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _umaServerEventSource = umaServerEventSource;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
        }

        public async Task<GetTokenByTicketIdResponse> Execute(GetTokenViaTicketIdParameter parameter, string openidProvider, string issuerName)
        {
            // 1. Check parameters.
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, PostAuthorizationNames.TicketId));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new ArgumentNullException(nameof(parameter.Ticket));
            }

            if (string.IsNullOrWhiteSpace(openidProvider))
            {
                throw new ArgumentNullException(nameof(openidProvider));
            }
            
            // 2. Retrieve the ticket.
            var json = JsonConvert.SerializeObject(parameter);
            _umaServerEventSource.StartGettingAuthorization(json);
            var ticket = await _ticketStore.GetAsync(parameter.Ticket).ConfigureAwait(false);
            if (ticket == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket, string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.Ticket));
            }

            // 3. Check the ticket.
            if (ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new BaseUmaException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
            }

            _umaServerEventSource.CheckAuthorizationPolicy(json);
            var claimTokenParameter = new ClaimTokenParameter
            {
                Token = parameter.ClaimToken,
                Format = parameter.ClaimTokenFormat
            };
            
            // 4. Check the authorization.
            var authorizationResult = await _authorizationPolicyValidator.IsAuthorized(openidProvider, ticket, claimTokenParameter).ConfigureAwait(false);
            if (!authorizationResult.IsValid)
            {
                _umaServerEventSource.RequestIsNotAuthorized(json);
                return new GetTokenByTicketIdResponse
                {
                    IsValid = false,
                    ResourceValidationResult = authorizationResult
                };
            }

            // 5. Generate a granted token.
            var grantedToken = await GenerateTokenAsync(ticket.Audiences, ticket.Lines, "openid", issuerName).ConfigureAwait(false);
            await _tokenStore.AddToken(grantedToken);
            await _ticketStore.RemoveAsync(ticket.Id);
            return new GetTokenByTicketIdResponse
            {
                IsValid = true,
                GrantedToken = grantedToken
            };
        }
        
        public async Task<GrantedToken> GenerateTokenAsync(IEnumerable<string> audiences, IEnumerable<TicketLine> ticketLines, string scope, string issuerName)
        {
            if (audiences == null)
            {
                throw new ArgumentNullException(nameof(audiences));
            }

            if (ticketLines == null)
            {
                throw new ArgumentNullException(nameof(ticketLines));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var expiresIn = await _configurationService.GetRptLifeTime().ConfigureAwait(false); // 1. Retrieve the expiration time of the granted token.
            var jwsPayload = await _jwtGenerator.GenerateAccessToken(audiences, scope.Split(' '), issuerName).ConfigureAwait(false); // 2. Construct the JWT token (client).
            var jArr = new JArray();
            foreach (var ticketLine in ticketLines)
            {
                var jObj = new JObject();
                jObj.Add(Constants.RptClaims.ResourceSetId, ticketLine.ResourceSetId);
                jObj.Add(Constants.RptClaims.Scopes, string.Join(" ",ticketLine.Scopes));
                jArr.Add(jObj);
            }

            jwsPayload.Add(Constants.RptClaims.Ticket, jArr);
            var clientId = audiences.First();
            var accessToken = await _clientHelper.GenerateIdTokenAsync(clientId, jwsPayload).ConfigureAwait(false);
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()); // 3. Construct the refresh token.
            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = expiresIn,
                TokenType = SimpleIdServer.Core.Constants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                Scope = scope,
                ClientId = clientId
            };
        }
    }
}
