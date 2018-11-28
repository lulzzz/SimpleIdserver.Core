using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Uma.Core.Website.ResourcesController;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    public class ResourcesController : BaseController
    {
        private readonly IResourcesActions _resourcesActions;

        public ResourcesController(IAuthenticationService  authenticationService, IResourcesActions resourcesActions) : base(authenticationService)
        {
            _resourcesActions = resourcesActions;
        }

        #region Actions

        [Authorize("connected")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        [Authorize("connected")]
        [HttpGet]
        public async Task<IActionResult> Shared()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        #endregion
    }
}