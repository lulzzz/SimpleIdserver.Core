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
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SimpleIdServer.Core.Authenticate;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;

namespace SimpleIdServer.Core.Api.Token.Actions
{
    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction : IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IScopeValidator _scopeValidator;
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IClientRepository _clientRepository;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IScopeValidator scopeValidator,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            IOAuthEventSource oauthEventSource,
            IAuthenticateClient authenticateClient,
            IJwtGenerator jwtGenerator,
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IClientRepository clientRepository,
            IClientHelper clientHelper,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
            _oauthEventSource = oauthEventSource;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _clientRepository = clientRepository;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = CreateAuthenticateInstruction(resourceOwnerGrantTypeParameter, authenticationHeaderValue, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName);
            var client = authResult.Client;
            if (authResult.Client == null)
            {                
                _oauthEventSource.Info(authResult.ErrorMessage);
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check the client.
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.password))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.password));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseType.token) || !client.ResponseTypes.Contains(ResponseType.id_token))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.ClientId, "token id_token"));
            }

            // 3. Try to authenticate a resource owner
            var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(resourceOwnerGrantTypeParameter.UserName, 
                resourceOwnerGrantTypeParameter.Password,
                resourceOwnerGrantTypeParameter.AmrValues);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant, ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            // 4. Check if the requested scopes are valid
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(resourceOwnerGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new IdentityServerException(ErrorCodes.InvalidScope, scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 5. Generate the user information payload and store it.
            var claims = resourceOwner.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "simpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = resourceOwnerGrantTypeParameter.Scope
            };
            var payload = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claimsPrincipal, authorizationParameter, issuerName);
            var generatedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId, payload, payload);
            if (generatedToken == null)
            {
                generatedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client, allowedTokenScopes, issuerName, payload, payload);
                if (generatedToken.IdTokenPayLoad != null)
                {
                    await _jwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad);
                    generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(client, generatedToken.IdTokenPayLoad);
                }

                await _tokenStore.AddToken(generatedToken);
                _oauthEventSource.GrantAccessToClient(client.ClientId, generatedToken.AccessToken, allowedTokenScopes);
            }

            return generatedToken;
        }
        
        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = resourceOwnerGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = resourceOwnerGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientSecret;
            result.Certificate = certificate;
            return result;
        }

        #endregion
    }
}