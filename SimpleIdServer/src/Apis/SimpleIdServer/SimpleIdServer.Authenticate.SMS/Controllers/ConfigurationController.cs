using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Authenticate.Basic.DTOs;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Authenticate.SMS.Controllers
{
    [Area(Constants.AMR)]
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = new AuthConfigurationResponse
            {
                Configurations = new string[0],
                Credential = new AuthCredentialConfigurationResponse
                {
                    IsEditable = false,
                    EditUrl = issuer + Url.Action("Index", "Credentials", new { area = Constants.AMR }),
                    Fields = new AuthCredentialFieldResponse[0]
                }
            };
            return new OkObjectResult(result);
        }
    }
}