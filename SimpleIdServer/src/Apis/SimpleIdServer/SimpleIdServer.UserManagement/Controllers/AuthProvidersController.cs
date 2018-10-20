using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.UserManagement.Extensions;

namespace SimpleIdServer.UserManagement.Controllers
{
    [Route("authproviders")]
    public class AuthProvidersController : Controller
    {
        protected readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public AuthProvidersController(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var result = schemes.ToDtos();
            return new OkObjectResult(result);
        }
    }
}
