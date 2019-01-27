using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Module;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                foreach(var authModule in _authModules)
                {
                    dic.Add(authModule.Name, Url.Action(authModule.ConfigurationUrl.ActionName, authModule.ConfigurationUrl.ControllerName,
                        new { area = authModule.ConfigurationUrl.Area }));
                }
            }

            return new OkObjectResult(dic);
        }
    }
}
