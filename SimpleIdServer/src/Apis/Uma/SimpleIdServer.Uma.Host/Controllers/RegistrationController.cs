using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Registration;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Uma.Host.Extensions;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Registration)]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [HttpPost]
        [Authorize("register_client")]
        public async Task<IActionResult> Post([FromBody] ClientRequest client)
        {
            if (client == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _registerActions.PostRegistration(client.ToParameter()).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
