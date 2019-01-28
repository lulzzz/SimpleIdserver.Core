using Microsoft.AspNetCore.Mvc;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            var result = new[]
            {
                "is_regex_enabled",
                "regex",
                "pwd_description"
            };
            return new OkObjectResult(result);
        }
    }
}