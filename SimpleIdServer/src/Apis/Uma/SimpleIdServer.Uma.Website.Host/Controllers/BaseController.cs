using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Uma.Website.Host.Extensions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;

        public BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        protected async Task RefreshViewBag()
        {
            var user = await GetUser().ConfigureAwait(false);
            ViewBag.IsAuthenticated = user != null && user.Identity != null && user.Identity.IsAuthenticated;
        }

        protected async Task<ClaimsPrincipal> GetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Constants.DEFAULT_COOKIE_NAME);
            return authenticatedUser;
        }
    }
}
