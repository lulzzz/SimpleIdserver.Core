using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Client;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.Jwt.Extensions;
using SimpleIdServer.Uma.Website.Host.Extensions;
using SimpleIdServer.Uma.Website.Host.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    public class AuthenticateController : BaseController
    {
        private class AuthorizationResponse
        {
            public string AuthorizationUrl { get; set; }
            public CookieContent CookieContent { get; set; }
        }

        private class CookieContent
        {
            public string Nonce { get; set; }
            public string State { get; set; }
        }

        private readonly UmaAuthenticationWebsiteOptions _options;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly JwsParserFactory _jwsParserFactory;

        public AuthenticateController(UmaAuthenticationWebsiteOptions options, IAuthenticationService  authenticationService) : base(authenticationService)
        {
            _options = options;
            _identityServerClientFactory = new IdentityServerClientFactory();
            _jwsParserFactory = new JwsParserFactory();
        }

        #region Actions

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAuthenticated = false;
            var authorizationResponse = await BuildAuthorizationUrl().ConfigureAwait(false);
            Response.Cookies.Append(Constants.TMP_COOKIE_NAME, JsonConvert.SerializeObject(authorizationResponse.CookieContent), new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5)
            });
            var viewModel = new AuthenticateViewModel(authorizationResponse.AuthorizationUrl);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string code, string state)
        {
            if (!Request.Cookies.ContainsKey(Constants.TMP_COOKIE_NAME))
            {
                return new UnauthorizedResult();
            }

            var cookieContent = JsonConvert.DeserializeObject<CookieContent>(Request.Cookies[Constants.TMP_COOKIE_NAME]);
            if (cookieContent.State != state)
            {
                return new UnauthorizedResult();
            }

            var redirectUrl = $"{Request.GetAbsoluteUriWithVirtualPath()}/Authenticate/Callback";
            var grantedToken = await _identityServerClientFactory.CreateAuthSelector().UseClientSecretPostAuth(_options.ClientId, _options.ClientSecret)
                .UseAuthorizationCode(code, redirectUrl)
                .ResolveAsync(_options.OpenidWellKnownConfigurationUrl).ConfigureAwait(false);
            if (grantedToken.ContainsError)
            {
                return new UnauthorizedResult();
            }

            if (!await AddCookie(grantedToken.Content.IdToken, cookieContent.Nonce).ConfigureAwait(false))
            {
                return new UnauthorizedResult();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Disconnect()
        {
            await _authenticationService.SignOutAsync(HttpContext, Constants.DEFAULT_COOKIE_NAME, new AuthenticationProperties()).ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Private methods

        private async Task<AuthorizationResponse> BuildAuthorizationUrl()
        {
            var discoveryInformation = await _identityServerClientFactory.CreateDiscoveryClient().GetDiscoveryInformationAsync(_options.OpenidWellKnownConfigurationUrl).ConfigureAwait(false);
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            var redirectUrl = $"{Request.GetAbsoluteUriWithVirtualPath()}/Authenticate/Callback";
            var url = $"{discoveryInformation.AuthorizationEndPoint}?scope=openid profile&state={state}&redirect_uri={redirectUrl}&response_type=code&client_id={_options.ClientId}&nonce={nonce}&response_mode=query&amr_values=pwd";
            return new AuthorizationResponse
            {
                AuthorizationUrl = url,
                CookieContent = new CookieContent
                {
                    State = state,
                    Nonce = nonce
                }
            };
        }

        private async Task<bool> AddCookie(string idToken, string nonce = null)
        {
            var jwsParser = _jwsParserFactory.BuildJwsParser();
            var claims = new List<Claim>();
            var payload = jwsParser.GetPayload(idToken);
            if (!string.IsNullOrWhiteSpace(nonce))
            {
                if (!payload.ContainsKey("nonce"))
                {
                    return false;
                }

                if (payload["nonce"].ToString() != nonce)
                {
                    return false;
                }
            }

            foreach (var kvp in payload)
            {
                claims.AddRange(Convert(kvp));
            }

            var claimsIdentity = new ClaimsIdentity(claims, Constants.DEFAULT_COOKIE_NAME);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await _authenticationService.SignInAsync(HttpContext, Constants.DEFAULT_COOKIE_NAME, claimsPrincipal, new AuthenticationProperties()).ConfigureAwait(false);
            return true;
        }

        private static IEnumerable<Claim> Convert(KeyValuePair<string, object> kvp)
        {
            var result = new List<Claim>();
            var arr = kvp.Value as JArray;
            if (arr == null)
            {
                result = new List<Claim>
                {
                    new Claim(kvp.Key, kvp.Value.ToString())
                };
            }
            else
            {
                foreach (var r in arr)
                {
                    result.Add(new Claim(kvp.Key, r.ToString()));
                }
            }

            return result.ToClaims();
        }

        #endregion
    }
}