using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Authenticate.SMS.Actions;
using SimpleIdServer.Authenticate.SMS.Common.Requests;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.SMS.Controllers
{
    [Route(Constants.CodeController)]
    public class CodeController : Controller
    {
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;

        public CodeController(ISmsAuthenticationOperation smsAuthenticationOperation)
        {
            _smsAuthenticationOperation = smsAuthenticationOperation;
        }

        /// <summary>
        /// Send the confirmation code to the phone number.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ConfirmationCodeRequest confirmationCodeRequest)
        {
            var checkResult = Check(confirmationCodeRequest);
            if (checkResult != null)
            {
                return checkResult;
            }

            IActionResult result = null;
            try
            {
                await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber);
                result = new OkResult();
            }
            catch(IdentityServerException ex)
            {
                result = BuildError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch(Exception)
            {
                result = BuildError(ErrorCodes.UnhandledExceptionCode, "unhandled exception occured please contact the administrator", HttpStatusCode.InternalServerError);
            }

            return result;
        }

        /// <summary>
        /// Check the parameter.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        private IActionResult Check(ConfirmationCodeRequest confirmationCodeRequest)
        {
            if (confirmationCodeRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no request", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, $"parameter {SMS.Common.Constants.ConfirmationCodeRequestNames.PhoneNumber} is missing", HttpStatusCode.BadRequest);
            }

            return null;
        }

        /// <summary>
        /// Build the JSON error message.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
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