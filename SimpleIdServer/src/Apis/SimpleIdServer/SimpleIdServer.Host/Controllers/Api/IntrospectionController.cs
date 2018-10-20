﻿#region copyright
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

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Core.Api.Introspection;
using SimpleIdServer.Core.Common.DTOs.Requests;
using SimpleIdServer.Core.Common.Serializers;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.Introspection)]
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
            catch(Exception)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }
            
            var nameValueCollection = new NameValueCollection();
            foreach(var kvp in Request.Form)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }

            var s = Thread.CurrentThread.ManagedThreadId;
            var serializer = new ParamSerializer();
            var introspectionRequest = serializer.Deserialize<IntrospectionRequest>(nameValueCollection);
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
