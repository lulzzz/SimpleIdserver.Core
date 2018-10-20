using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Host.Controllers.Website;

namespace SimpleIdServer.Shell.Controllers
{
    [Area("Shell")]
    public class HomeController : BaseController
    {
        public HomeController(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser();
            return View();
        }

        #endregion
    }
}
