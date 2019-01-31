using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Authenticate.LoginPassword.Actions;
using SimpleIdServer.Authenticate.LoginPassword.DTOs;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class CredentialsController : Controller
    {
        private readonly IChangePasswordAction _changePasswordAction;

        public CredentialsController(IChangePasswordAction changePasswordAction)
        {
            _changePasswordAction = changePasswordAction;
        }

        [HttpPut]
        [Authorize("edit_credentials")]
        public async Task<IActionResult> Index([FromBody] EditLoginPasswordRequest editLoginPasswordRequest)
        {
            if (editLoginPasswordRequest == null)
            {
                throw new ArgumentNullException(nameof(editLoginPasswordRequest));
            }

            try
            {
                await _changePasswordAction.Execute(new ChangePasswordParameter
                {
                    ActualPassword = editLoginPasswordRequest.ActualPassword,
                    NewPassword = editLoginPasswordRequest.NewPassword,
                    Subject = editLoginPasswordRequest.Subject
                }).ConfigureAwait(false);
                return new OkResult();
            }
            catch(Exception ex)
            {
                return BuildError("invalid_request", ex.Message, HttpStatusCode.BadRequest);
            }
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new SimpleIdServer.Common.Dtos.Responses.ErrorResponse
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
