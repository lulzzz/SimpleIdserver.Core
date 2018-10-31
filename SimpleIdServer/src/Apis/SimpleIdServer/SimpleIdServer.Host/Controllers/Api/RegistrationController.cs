#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Registration;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Host.Extensions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.Registration)]
    [Authorize("registration")]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClientRequest client)
        {
            if (client == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = client.ToParameter();
            parameter.Scopes = new List<Core.Common.Models.Scope>
            {
                Core.Constants.StandardScopes.OpenId,
                Core.Constants.StandardScopes.ProfileScope,
                Core.Constants.StandardScopes.Address,
                Core.Constants.StandardScopes.Email,
                Core.Constants.StandardScopes.Phone
            };
            var result = await _registerActions.PostRegistration(client.ToParameter());
            return new OkObjectResult(result);
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