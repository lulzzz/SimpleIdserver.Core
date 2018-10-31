using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Introspection;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Lib;
using SimpleIdServer.Uma.Host.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Introspection)]
    public class IntrospectionController : Controller
    {
        private readonly IIntrospectionActions _introspectionActions;

        public IntrospectionController(IIntrospectionActions introspectionActions)
        {
            _introspectionActions = introspectionActions;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {

                if (Request.Form == null)
                {
                    return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var serializer = new ParamSerializer();
            var introspectionRequest = serializer.Deserialize<IntrospectionRequest>(Request.Form);
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _introspectionActions.PostIntrospection(introspectionRequest.ToParameter(), authenticationHeaderValue, issuerName);
            return new OkObjectResult(result.ToDto());
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
