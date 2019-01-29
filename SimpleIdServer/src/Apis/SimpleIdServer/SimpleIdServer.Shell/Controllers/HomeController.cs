using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Client;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.Jwt.Extensions;
using SimpleIdServer.Host.Controllers.Website;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.Module;
using SimpleIdServer.Shell.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Shell.Controllers
{
    [Area("Shell")]
    public class HomeController : BaseController
    {
        private static string COOKIE_NAME = "TMP_SIMPLEIDSERVER";
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

        private readonly IEnumerable<IUiModule> _uiModules;
        private readonly ShellModuleOptions _shellModuleOptions;

        public HomeController(IAuthenticationService authenticationService, IEnumerable<IUiModule> uiModules, ShellModuleOptions shellModuleOptions) : base(authenticationService)
        {
            _uiModules = uiModules;
            _shellModuleOptions = shellModuleOptions;
        }

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            var viewModel = new HomeViewModel
            {
                Tiles = _uiModules.Select(u => new HomeTileViewModel
                {
                    DisplayName = u.DisplayName,
                    Picture = u.Picture,
                    RedirectionUrl = u.RedirectionUrl
                })
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Authenticate()
        {
            await SetUser().ConfigureAwait(false);
            var auth = await BuildAuthorizationUrl().ConfigureAwait(false);
            Response.Cookies.Append(COOKIE_NAME, JsonConvert.SerializeObject(auth.CookieContent), new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(30)
            });
            var viewModel = new AuthenticateViewModel
            {
                AuthUrl = auth.AuthorizationUrl
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Callback(string code, string state)
        {
            if (!Request.Cookies.ContainsKey(COOKIE_NAME))
            {
                return new UnauthorizedResult();
            }

            var cookieContent = JsonConvert.DeserializeObject<CookieContent>(Request.Cookies[COOKIE_NAME]);
            if (cookieContent.State != state)
            {
                return new UnauthorizedResult();
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var wellKnownConfiguration = $"{issuerName}/.well-known/openid-configuration";
            var identityServerClientFactory = new IdentityServerClientFactory();
            var grantedToken = await identityServerClientFactory.CreateAuthSelector().UseClientSecretPostAuth(_shellModuleOptions.ClientId, _shellModuleOptions.ClientSecret)
                .UseAuthorizationCode(code, issuerName + Url.Action("Callback"))
                .ResolveAsync(wellKnownConfiguration).ConfigureAwait(false);
            if (grantedToken.ContainsError)
            {
                return new UnauthorizedResult();
            }

            if (!await AddCookie(grantedToken.Content.IdToken, cookieContent.Nonce).ConfigureAwait(false))
            {
                return new UnauthorizedResult();
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete(SimpleIdServer.Core.Constants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.CookieNames.CookieName, new AuthenticationProperties());
            return RedirectToAction("Index", "Home");
        }

        #endregion

        private async Task<bool> AddCookie(string idToken, string nonce = null)
        {
            var jwsParserFactory = new JwsParserFactory();
            var jwsParser = jwsParserFactory.BuildJwsParser();
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

            var claimsIdentity = new ClaimsIdentity(claims, Host.Constants.CookieNames.CookieName);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await _authenticationService.SignInAsync(HttpContext, Host.Constants.CookieNames.CookieName, claimsPrincipal, new AuthenticationProperties()).ConfigureAwait(false);
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

        private async Task<AuthorizationResponse> BuildAuthorizationUrl()
        {
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var wellKnownConfiguration = $"{issuerName}/.well-known/openid-configuration";
            var identityServerClientFactory = new IdentityServerClientFactory();
            var discoveryInformation = await identityServerClientFactory.CreateDiscoveryClient().GetDiscoveryInformationAsync(wellKnownConfiguration).ConfigureAwait(false);
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            var url = $"{discoveryInformation.AuthorizationEndPoint}?scope=openid profile&state={state}&redirect_uri={issuerName + Url.Action("Callback")}&response_type=code&client_id={_shellModuleOptions.ClientId}&nonce={nonce}&response_mode=query&acr_values=sid::loa-1";
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
    }
}
