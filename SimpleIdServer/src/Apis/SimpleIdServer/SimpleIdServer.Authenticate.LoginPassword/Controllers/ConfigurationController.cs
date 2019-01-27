using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            return new OkObjectResult(new PwdCredentialOptions());
        }
    }
}