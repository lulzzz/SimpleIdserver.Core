using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Dtos.Responses;
using SimpleIdServer.OAuth.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Registration.Actions
{
    public interface IRegisterClientAction
    {
        Task<ClientRegistrationResponse> Execute(RegistrationParameter registrationParameter);
    }

    public class RegisterClientAction : IRegisterClientAction
    {
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IClientRepository _clientRepository;
        private readonly IGenerateClientFromRegistrationRequest _generateClientFromRegistrationRequest;
        private readonly IClientPasswordService _clientPasswordService;
        private readonly IClientInfoService _clientInfoService;

        public RegisterClientAction(IOAuthEventSource oauthEventSource, IClientRepository clientRepository, IGenerateClientFromRegistrationRequest generateClientFromRegistrationRequest, 
            IClientPasswordService clientPasswordService, IClientInfoService clientInfoService)
        {
            _oauthEventSource = oauthEventSource;
            _clientRepository = clientRepository;
            _generateClientFromRegistrationRequest = generateClientFromRegistrationRequest;
            _clientPasswordService = clientPasswordService;
            _clientInfoService = clientInfoService;
        }

        public async Task<ClientRegistrationResponse> Execute(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException(nameof(registrationParameter));
            }

            _oauthEventSource.StartRegistration(registrationParameter.ClientName);
            var clientId = await _clientInfoService.GetClientId().ConfigureAwait(false);
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);
            client.AllowedScopes = registrationParameter.Scopes;
            client.ClientId = clientId;
            var result = new ClientRegistrationResponse
            {
                ClientId = clientId,
                ClientSecretExpiresAt = 0,
                ClientIdIssuedAt = DateTime.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                ApplicationType = Enum.GetName(typeof(ApplicationTypes), client.ApplicationType),
                ClientUri = GetDefaultValue(client.ClientUri),
                ClientName = GetDefaultValue(client.ClientName),
                Contacts = GetDefaultValues(client.Contacts).ToArray(),
                DefaultAcrValues = GetDefaultValue(client.DefaultAcrValues),
                GrantTypes = client.GrantTypes == null ? new string[0] : client.GrantTypes.Select(g => Enum.GetName(typeof(GrantType), g)).ToArray(),
                DefaultMaxAge = client.DefaultMaxAge,
                IdTokenEncryptedResponseAlg = GetDefaultValue(client.IdTokenEncryptedResponseAlg),
                IdTokenEncryptedResponseEnc = GetDefaultValue(client.IdTokenEncryptedResponseEnc),
                JwksUri = GetDefaultValue(client.JwksUri),
                RequestObjectEncryptionAlg = GetDefaultValue(client.RequestObjectEncryptionAlg),
                RequestObjectEncryptionEnc = GetDefaultValue(client.RequestObjectEncryptionEnc),
                IdTokenSignedResponseAlg = GetDefaultValue(client.IdTokenSignedResponseAlg),
                LogoUri = GetDefaultValue(client.LogoUri),
                Jwks = registrationParameter.Jwks,
                RequireAuthTime = client.RequireAuthTime,
                InitiateLoginUri = GetDefaultValue(client.InitiateLoginUri),
                PolicyUri = GetDefaultValue(client.PolicyUri),
                RequestObjectSigningAlg = GetDefaultValue(client.RequestObjectSigningAlg),
                UserInfoEncryptedResponseAlg = GetDefaultValue(client.UserInfoEncryptedResponseAlg),
                UserInfoEncryptedResponseEnc = GetDefaultValue(client.UserInfoEncryptedResponseEnc),
                UserInfoSignedResponseAlg = GetDefaultValue(client.UserInfoSignedResponseAlg),
                TosUri = GetDefaultValue(client.TosUri),
                SectorIdentifierUri = GetDefaultValue(client.SectorIdentifierUri),
                SubjectType = GetDefaultValue(client.SubjectType),
                ResponseTypes = client.ResponseTypes == null ? new string[0] : client.ResponseTypes.Select(r => Enum.GetName(typeof(ResponseType), r)).ToArray(),
                RequestUris = GetDefaultValues(client.RequestUris).ToList(),
                RedirectUris = GetDefaultValues(client.RedirectionUrls).ToArray(),
                PostLogoutRedirectUris = GetDefaultValues(client.PostLogoutRedirectUris).ToArray(),
                TokenEndpointAuthSigningAlg = GetDefaultValue(client.TokenEndPointAuthSigningAlg),
                TokenEndpointAuthMethod = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), client.TokenEndPointAuthMethod),
                ScimProfile = client.ScimProfile,
                RequirePkce = client.RequirePkce
            };

            if (client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.private_key_jwt)
            {
                result.ClientSecret = await _clientInfoService.GetClientSecret().ConfigureAwait(false);
                client.Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = _clientPasswordService.Encrypt(result.ClientSecret)
                    }
                };
            }

            await _clientRepository.InsertAsync(client).ConfigureAwait(false);
            _oauthEventSource.EndRegistration(result.ClientId, client.ClientName);
            return result;
        }

        private static string GetDefaultValue(string value)
        {
            return value == null ? string.Empty : value;
        }

        private static IEnumerable<string> GetDefaultValues(IEnumerable<string> value)
        {
            return value == null ? new string[0] : value;
        }
    }
}