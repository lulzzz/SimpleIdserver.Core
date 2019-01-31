using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Host.Extensions;
using SimpleIdServer.Module;
using System.Collections.Generic;

namespace SimpleIdServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.ModulesConfiguration)]
    public class ModulesConfigurationController : Controller
    {
        private readonly IEnumerable<IAuthModule> _authModules;

        public ModulesConfigurationController(IEnumerable<IAuthModule> authModules)
        {
            _authModules = authModules;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var dic = new Dictionary<string, string>();
            if (_authModules != null)
            {
                var issuer = Request.GetAbsoluteUriWithVirtualPath();
                foreach (var authModule in _authModules)
                {
                    dic.Add(authModule.Name, issuer + Url.Action(authModule.ConfigurationUrl.ActionName, authModule.ConfigurationUrl.ControllerName,
                        new { area = authModule.ConfigurationUrl.Area }));
                }
            }

            return new OkObjectResult(dic);
        }
    }
}
