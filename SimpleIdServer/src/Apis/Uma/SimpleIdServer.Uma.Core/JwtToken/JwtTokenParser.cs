using SimpleIdServer.Client;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Jwt.Converter;
using SimpleIdServer.Core.Jwt.Signature;
using SimpleIdServer.Uma.Core.Models;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.JwtToken
{
    public interface IJwtTokenParser
    {
        /// <summary>
        /// Unsign the JWS
        /// </summary>
        /// <param name="jws"></param>
        /// <param name="policyRule"></param>
        /// <returns></returns>
        Task<JwsPayload> UnSign(string jws, PolicyRule policyRule);
    }

    internal class JwtTokenParser : IJwtTokenParser
    {
        private readonly IJwsParser _jwsParser;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public JwtTokenParser(IJwsParser jwsParser, IJsonWebKeyConverter jsonWebKeyConverter, IIdentityServerClientFactory identityServerClientFactory)
        {
            _jwsParser = jwsParser;
            _jsonWebKeyConverter = jsonWebKeyConverter;
            _identityServerClientFactory = identityServerClientFactory;
        }

        public async Task<JwsPayload> UnSign(string jws, PolicyRule policyRule)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (policyRule == null)
            {
                throw new ArgumentNullException(nameof(policyRule));
            }

            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }
            
            if (protectedHeader.Alg == SimpleIdServer.Core.Jwt.Constants.JwsAlgNames.NONE)
            {
                return _jwsParser.GetPayload(jws);
            }

            var jsonWebKeySet = await _identityServerClientFactory.CreateJwksClient().ResolveAsync(policyRule.OpenIdProvider).ConfigureAwait(false);
            return _jwsParser.ValidateSignature(jws, jsonWebKeySet);
        }
    }
}
