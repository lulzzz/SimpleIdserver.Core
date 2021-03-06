﻿using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.JwtToken;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Helpers
{
    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Common.Models.Client client, JwsPayload jwsPayload);
        Task<JwsPayload> GetPayload(string clientId, string jwsToken);
        Task<JwsPayload> GetPayload(Common.Models.Client client, string jwsToken);
    }

    public sealed class ClientHelper : IClientHelper
    {
        private readonly IClientRepository _clientRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IJwtParser _jwtParser;

        public ClientHelper(IClientRepository clientRepository, IJwtGenerator jwtGenerator, IJwtParser jwtParser)
        {
            _clientRepository = clientRepository;
            _jwtGenerator = jwtGenerator;
            _jwtParser = jwtParser;
        }

        public async Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return null;
            }

            return await GenerateIdTokenAsync(client, jwsPayload);
        }

        public async Task<string> GenerateIdTokenAsync(Core.Common.Models.Client client, JwsPayload jwsPayload)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var signedResponseAlg = client.GetIdTokenSignedResponseAlg();
            var encryptResponseAlg = client.GetIdTokenEncryptedResponseAlg();
            var encryptResponseEnc = client.GetIdTokenEncryptedResponseEnc();
            if (signedResponseAlg == null)
            {
                signedResponseAlg = JwsAlg.RS256;
            }

            var idToken = await _jwtGenerator.SignAsync(jwsPayload, signedResponseAlg.Value);
            if (encryptResponseAlg == null)
            {
                return idToken;
            }

            if (encryptResponseEnc == null)
            {
                encryptResponseEnc = JweEnc.A128CBC_HS256;
            }

            return await _jwtGenerator.EncryptAsync(idToken, encryptResponseAlg.Value, encryptResponseEnc.Value);
        }

        public async Task<JwsPayload> GetPayload(string clientId, string jwsToken)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(jwsToken))
            {
                throw new ArgumentNullException(nameof(jwsToken));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return null;
            }

            return await GetPayload(client, jwsToken);
        }

        public async Task<JwsPayload> GetPayload(Core.Common.Models.Client client, string jwsToken)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(jwsToken))
            {
                throw new ArgumentNullException(nameof(jwsToken));
            }


            var signedResponseAlg = client.GetIdTokenSignedResponseAlg();
            var encryptResponseAlg = client.GetIdTokenEncryptedResponseAlg();
            if (encryptResponseAlg != null) // Decrypt the token.
            {
                jwsToken = await _jwtParser.DecryptAsync(jwsToken, client.ClientId);
            }

            return await _jwtParser.UnSignAsync(jwsToken, client.ClientId);
        }
    }
}
