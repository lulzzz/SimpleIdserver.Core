using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Token;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Lib;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.Token)]
    public class TokenController : Controller
    {
        private readonly ITokenActions _tokenActions;

        public TokenController(
            ITokenActions tokenActions)
        {
            _tokenActions = tokenActions;
        }

        [HttpPost]
        public async Task<IActionResult> PostToken()
        {
            var certificate = GetCertificate();
            try
            {
                if (Request.Form == null)
                {
                    return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
                }
            }
            catch(Exception)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var serializer = new ParamSerializer();
            var tokenRequest = serializer.Deserialize<TokenRequest>(Request.Form);
            if (tokenRequest.GrantType == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, Dtos.Constants.RequestTokenNames.GrantType), HttpStatusCode.BadRequest);
            }

            GrantedToken result = null;
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader)) 
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            switch (tokenRequest.GrantType)
            {
                case Dtos.Requests.GrantTypes.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case Dtos.Requests.GrantTypes.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = await _tokenActions.GetTokenByAuthorizationCodeGrantType(authCodeParameter,authenticationHeaderValue, certificate, issuerName);
                    break;
                case Dtos.Requests.GrantTypes.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = await _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case Dtos.Requests.GrantTypes.client_credentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = await _tokenActions.GetTokenByClientCredentialsGrantType(clientCredentialsParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
            }

            return new OkObjectResult(result.ToDto());
        }

        [HttpPost("revoke")]
        public async Task<ActionResult> PostRevoke()
        {
            try
            {
                if (Request.Form == null)
                {
                    return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var nameValueCollection = new NameValueCollection();
            foreach (var kvp in Request.Form)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }

            var serializer = new ParamSerializer();
            var revocationRequest = serializer.Deserialize<RevocationRequest>(nameValueCollection);
            // 1. Fetch the authorization header
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            // 2. Revoke the token
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            await _tokenActions.RevokeToken(revocationRequest.ToParameter(), authenticationHeaderValue, GetCertificate(), issuerName);
            return new OkResult();
        }

        private X509Certificate2 GetCertificate()
        {
            const string headerName = "X-ARR-ClientCert";
            var header = Request.Headers.FirstOrDefault(h => h.Key == headerName);
            if (header.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header.Value);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Build the JSON error message.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
