﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SimpleIdServer.Core.Api.Authorization;
using SimpleIdServer.Lib;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;
using Newtonsoft.Json;
using SimpleIdServer.Core.Common.Repositories;

namespace SimpleIdServer.Core.Common
{
    public interface IGenerateAuthorizationResponse
    {
        Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedSubject);
    }

    public class GenerateAuthorizationResponse : IGenerateAuthorizationResponse
    {
        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly ITokenStore _tokenStore;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IConsentHelper _consentHelper;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IClientHelper _clientHelper;
        private readonly IGrantedTokenHelper _grantedTokenHelper;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IParameterParserHelper parameterParserHelper,
            IJwtGenerator jwtGenerator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IConsentHelper consentHelper,
            IOAuthEventSource oauthEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IClientHelper clientHelper,
            IGrantedTokenHelper grantedTokenHelper,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _authorizationCodeStore = authorizationCodeStore;
            _tokenStore = tokenStore;
            _parameterParserHelper = parameterParserHelper;
            _jwtGenerator = jwtGenerator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _consentHelper = consentHelper;
            _oauthEventSource = oauthEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _clientHelper = clientHelper;
            _grantedTokenHelper = grantedTokenHelper;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, Client client, string issuerName, string authenticatedSubject)
        {
            if (actionResult == null || actionResult.RedirectInstruction == null)
            {
                throw new ArgumentNullException(nameof(actionResult));
            }
;
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authenticatedSubject))
            {
                throw new ArgumentNullException(nameof(authenticatedSubject));
            }

            var newAccessTokenGranted = false;
            var allowedTokenScopes = string.Empty;
            GrantedToken grantedToken = null;
            var newAuthorizationCodeGranted = false;
            AuthorizationCode authorizationCode = null;
            _oauthEventSource.StartGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
                authorizationParameter.ResponseType);
            var responses = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            var user = await _resourceOwnerRepository.GetAsync(authenticatedSubject).ConfigureAwait(false);
            var claims = user.Claims;
            var idTokenPayload = await GenerateIdTokenPayload(claims, authorizationParameter, issuerName).ConfigureAwait(false);
            var userInformationPayload = await GenerateUserInformationPayload(claims, authorizationParameter).ConfigureAwait(false);
            if (responses.Contains(ResponseType.token)) // 1. Generate an access token.
            {
                if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                {
                    allowedTokenScopes = string.Join(" ", _parameterParserHelper.ParseScopes(authorizationParameter.Scope));
                }

                
                grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId,
                    userInformationPayload, idTokenPayload);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client, allowedTokenScopes,
                        issuerName, userInformationPayload, idTokenPayload);
                    newAccessTokenGranted = true;
                }

                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AccessTokenName,
                    grantedToken.AccessToken);
            }

            if (responses.Contains(ResponseType.code)) // 2. Generate an authorization code.
            {
                var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(authenticatedSubject, authorizationParameter).ConfigureAwait(false);
                if (assignedConsent != null)
                {
                    // Insert a temporary authorization code 
                    // It will be used later to retrieve tha id_token or an access token.
                    authorizationCode = new AuthorizationCode
                    {
                        Code = Guid.NewGuid().ToString(),
                        RedirectUri = authorizationParameter.RedirectUrl,
                        CreateDateTime = DateTime.UtcNow,
                        ClientId = authorizationParameter.ClientId,
                        Scopes = authorizationParameter.Scope,
                        IdTokenPayload = idTokenPayload,
                        UserInfoPayLoad = userInformationPayload
                    };

                    newAuthorizationCodeGranted = true;
                    actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                        authorizationCode.Code);
                }
            }

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(idTokenPayload,
                authorizationCode == null ? string.Empty : authorizationCode.Code,
                grantedToken == null ? string.Empty : grantedToken.AccessToken,
                authorizationParameter, client);
            
            if (newAccessTokenGranted) // 3. Insert the stateful access token into the DB OR insert the access token into the caching.
            {
                await _tokenStore.AddToken(grantedToken);
                _oauthEventSource.GrantAccessToClient(authorizationParameter.ClientId,
                    grantedToken.AccessToken,
                    allowedTokenScopes);
            }

            if (newAuthorizationCodeGranted) // 4. Insert the authorization code into the caching.
            {
                if (client.RequirePkce)
                {
                    authorizationCode.CodeChallenge = authorizationParameter.CodeChallenge;
                    authorizationCode.CodeChallengeMethod = authorizationParameter.CodeChallengeMethod;
                }

                await _authorizationCodeStore.AddAuthorizationCode(authorizationCode);
                _oauthEventSource.GrantAuthorizationCodeToClient(authorizationParameter.ClientId,
                    authorizationCode.Code,
                    authorizationParameter.Scope);
            }

            if (responses.Contains(ResponseType.id_token))
            {
                var idToken = await GenerateIdToken(idTokenPayload, authorizationParameter);
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.IdTokenName, idToken);
            }

            if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
            {
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.StateName, authorizationParameter.State);
            }

            var sessionState = GetSessionState(authorizationParameter.ClientId, authorizationParameter.OriginUrl, authorizationParameter.SessionId);
            if (sessionState != null)
            {
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.SessionState, sessionState);
            }

            if (authorizationParameter.ResponseMode == ResponseMode.form_post)
            {
                actionResult.Type = TypeActionResult.RedirectToAction;
                actionResult.RedirectInstruction.Action = IdentityServerEndPoints.FormIndex;
                actionResult.RedirectInstruction.AddParameter("redirect_uri", authorizationParameter.RedirectUrl);
            }

            // Set the response mode
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes,
                        authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                actionResult.RedirectInstruction.ResponseMode = responseMode;
            }

            _oauthEventSource.EndGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
               JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters));
        }

        private string GetSessionState(string clientId, string originUrl, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(originUrl))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            var sessionState = string.Empty;
            var salt = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(clientId + originUrl + sessionId + salt);
            byte[] hash;
            using (var sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            var hex = ToHexString(hash);
            return hex.Base64Encode() + "==." + salt;
        }

        public static string ToHexString(IEnumerable<byte> arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            var sb = new StringBuilder();
            foreach (var s in arr)
            {
                sb.Append(s.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the JWS payload for identity token.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="jwsPayload"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<string> GenerateIdToken(JwsPayload jwsPayload, AuthorizationParameter authorizationParameter)
        {
            return await _clientHelper.GenerateIdTokenAsync(authorizationParameter.ClientId,
                jwsPayload);
        }

        private async Task<JwsPayload> GenerateIdTokenPayload(IList<Claim> claims, AuthorizationParameter authorizationParameter, string issuerName)
        {
            JwsPayload jwsPayload;
            if (authorizationParameter.Claims != null && 
                authorizationParameter.Claims.IsAnyIdentityTokenClaimParameter())
            {
                jwsPayload = await _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(claims, authorizationParameter, Clone(authorizationParameter.Claims.IdToken), issuerName);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claims, authorizationParameter, issuerName);
            }

            return jwsPayload;
        }

        /// <summary>
        /// Generate the JWS payload for user information endpoint.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<JwsPayload> GenerateUserInformationPayload(IList<Claim> claims, AuthorizationParameter authorizationParameter)
        {
            JwsPayload jwsPayload;
            if (authorizationParameter.Claims != null && authorizationParameter.Claims.IsAnyUserInfoClaimParameter())
            {
                jwsPayload = _jwtGenerator.GenerateFilteredUserInfoPayload(Clone(authorizationParameter.Claims.UserInfo), authorizationParameter, claims);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(authorizationParameter, claims);
            }

            return jwsPayload;
        }
        
        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }

        private static List<ClaimParameter> Clone(List<ClaimParameter> claims)
        {
            var result = new List<ClaimParameter>();
            claims.ForEach(c => result.Add(c));
            return result;
        }
    }
}
