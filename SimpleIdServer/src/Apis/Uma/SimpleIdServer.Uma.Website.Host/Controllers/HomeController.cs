using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IAuthenticationService  authenticationService) : base(authenticationService)
        {
        }

        #region Actions

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        #endregion
    }
}