using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Host.Controllers.Website
{
    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;

        public BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<ClaimsPrincipal> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieNames.CookieName);
            var isAuthenticed = authenticatedUser != null && authenticatedUser.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            if (isAuthenticed)
            {
                ViewBag.Name = authenticatedUser.GetName();
            }
            else
            {
                ViewBag.Name = "unknown";
            }

            return authenticatedUser;
        }
    }
}
