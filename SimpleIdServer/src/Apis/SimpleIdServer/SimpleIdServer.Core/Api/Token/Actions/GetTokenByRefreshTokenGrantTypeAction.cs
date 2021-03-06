﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SimpleIdServer.Core.Authenticate;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;

namespace SimpleIdServer.Core.Api.Token.Actions
{
    public interface IGetTokenByRefreshTokenGrantTypeAction
    {
        Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }

    public sealed class GetTokenByRefreshTokenGrantTypeAction : IGetTokenByRefreshTokenGrantTypeAction
    {
        private readonly IClientHelper _clientHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly ITokenStore _tokenStore;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;

        public GetTokenByRefreshTokenGrantTypeAction(
            IClientHelper clientHelper,
            IOAuthEventSource oauthEventSource,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            ITokenStore tokenStore,
            IJwtGenerator jwtGenerator,
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient)
        {
            _clientHelper = clientHelper;
            _oauthEventSource = oauthEventSource;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _tokenStore = tokenStore;
            _jwtGenerator = jwtGenerator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
        }

        public async Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = CreateAuthenticateInstruction(refreshTokenGrantTypeParameter, authenticationHeaderValue, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                _oauthEventSource.Info(authResult.ErrorMessage);
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.refresh_token))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.refresh_token));
            }

            // 3. Validate parameters
            var grantedToken = await ValidateParameter(refreshTokenGrantTypeParameter);
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer);
            }

            // 4. Generate a new access token & insert it
            var generatedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(
                grantedToken.ClientId,
                grantedToken.Scope,
                issuerName,
                grantedToken.UserInfoPayLoad,
                grantedToken.IdTokenPayLoad);
            generatedToken.ParentTokenId = grantedToken.Id;
            // 5. Fill-in the idtoken
            if (generatedToken.IdTokenPayLoad != null)
            {
                await _jwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad);
                generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(generatedToken.ClientId, generatedToken.IdTokenPayLoad);
            }

            await _tokenStore.AddToken(generatedToken);
            _oauthEventSource.GrantAccessToClient(generatedToken.ClientId,
                generatedToken.AccessToken,
                generatedToken.Scope);
            return generatedToken;
        }

        #region Private methods


        private async Task<GrantedToken> ValidateParameter(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            var grantedToken = await _tokenStore.GetRefreshToken(refreshTokenGrantTypeParameter.RefreshToken);
            if (grantedToken == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRefreshTokenIsNotValid);
            }

            return grantedToken;
        }

        private AuthenticateInstruction CreateAuthenticateInstruction(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = refreshTokenGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = refreshTokenGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = refreshTokenGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = refreshTokenGrantTypeParameter.ClientSecret;
            result.Certificate = certificate;
            return result;
        }

        #endregion
    }
}
