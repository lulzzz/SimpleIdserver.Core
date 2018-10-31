using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Core.Api.PermissionController;
using SimpleIdServer.Uma.Host.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Permission)]
    public class PermissionsController : Controller
    {
        private readonly IPermissionControllerActions _permissionControllerActions;

        public PermissionsController(IPermissionControllerActions permissionControllerActions)
        {
            _permissionControllerActions = permissionControllerActions;
        }

        [HttpPost]
        [Authorize("uma_protection")]
        public async Task<ActionResult> PostPermission([FromBody] PostPermission postPermission)
        {
            if (postPermission == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = postPermission.ToParameter();
            var clientId = this.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the client_id cannot be extracted", HttpStatusCode.BadRequest);
            }

            var ticketId = await _permissionControllerActions.Add(parameter, clientId);
            var result = new AddPermissionResponse
            {
                TicketId = ticketId
            };
            return new ObjectResult(result)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPost("bulk")]
        [Authorize("uma_protection")]
        public async Task<ActionResult> PostPermissions([FromBody] IEnumerable<PostPermission> postPermissions)
        {
            if (postPermissions == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameters = postPermissions.Select(p => p.ToParameter());
            var clientId = this.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the client_id cannot be extracted", HttpStatusCode.BadRequest);
            }

            var ticketId = await _permissionControllerActions.Add(parameters, clientId);
            var result = new AddPermissionResponse
            {
                TicketId = ticketId
            };
            return new ObjectResult(result)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
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
