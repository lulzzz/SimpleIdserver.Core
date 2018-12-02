using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Token;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Lib;
using SimpleIdServer.Uma.Core.Api.Token;
using SimpleIdServer.Uma.Core.Policies;
using SimpleIdServer.Uma.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Token)]
    public class TokenController : Controller
    {
        private readonly ITokenActions _tokenActions;
        private readonly IUmaTokenActions _umaTokenActions;
        private readonly AuthorizationServerOptions _authorizationServerOptions;

        public TokenController(ITokenActions tokenActions, IUmaTokenActions umaTokenActions, AuthorizationServerOptions authorizationServerOptions)
        {
            _tokenActions = tokenActions;
            _umaTokenActions = umaTokenActions;
            _authorizationServerOptions = authorizationServerOptions;
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
            catch (Exception)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var serializer = new ParamSerializer();
            var tokenRequest = serializer.Deserialize<TokenRequest>(Request.Form);
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
                case GrantTypes.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case GrantTypes.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = await _tokenActions.GetTokenByAuthorizationCodeGrantType(authCodeParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case GrantTypes.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = await _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case GrantTypes.client_credentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = await _tokenActions.GetTokenByClientCredentialsGrantType(clientCredentialsParameter, authenticationHeaderValue, certificate, issuerName);
                    break;
                case GrantTypes.uma_ticket:
                    var tokenIdParameter = tokenRequest.ToTokenIdGrantTypeParameter();
                    var getTokenByTicketIdResponse = await _umaTokenActions.GetTokenByTicketId(tokenIdParameter, _authorizationServerOptions.OpenidWellKnwonConfiguration, issuerName);
                    if (!getTokenByTicketIdResponse.IsValid)
                    {
                        var jArr = new JArray();
                        foreach (var policyResult in getTokenByTicketIdResponse.ResourceValidationResult.AuthorizationPoliciesResult)
                        {
                            if (policyResult.Type != AuthorizationPolicyResultEnum.Authorized)
                            {
                                continue;
                            }

                            var record = new JObject();
                            record.Add("policy_id", policyResult.Policy.Id);
                            record.Add("status", policyResult.Type.ToString());
                            if (policyResult.ErrorDetails != null)
                            {
                                record.Add("description", JObject.Parse(JsonConvert.SerializeObject(policyResult.ErrorDetails)));
                            }

                            jArr.Add(record);
                        }

                        return new JsonResult(jArr)
                        {
                            StatusCode = (int)HttpStatusCode.InternalServerError
                        };
                    }

                    result = getTokenByTicketIdResponse.GrantedToken;
                    break;
            }

            return new OkObjectResult(result.ToDto());
        }

        [HttpPost("revoke")]
        public async Task<ActionResult> PostRevoke()
        {
            if (Request.Form == null)
            {
                throw new ArgumentNullException(nameof(Request.Form));
            }

            var serializer = new ParamSerializer();
            var revocationRequest = serializer.Deserialize<RevocationRequest>(Request.Form);
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
