using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Authenticate.Basic.DTOs;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = new AuthConfigurationResponse
            {
                Configurations = new[]
                {
                    "is_regex_enabled",
                    "regex",
                    "pwd_description"
                },
                Credential = new AuthCredentialConfigurationResponse
                {
                    IsEditable = true,
                    EditUrl = issuer + Url.Action("Index", "Credentials", new { area = Constants.AMR }),
                    Fields = new []
                    {
                        new AuthCredentialFieldResponse
                        {
                            Name = "actual_password",
                            DisplayName = "Current password",
                            Type = "pwd"
                        },
                        new AuthCredentialFieldResponse
                        {
                            Name = "new_password",
                            DisplayName = "New password",
                            Type = "pwd"
                        }
                    }
                }
            };
            return new OkObjectResult(result);
        }
    }
}