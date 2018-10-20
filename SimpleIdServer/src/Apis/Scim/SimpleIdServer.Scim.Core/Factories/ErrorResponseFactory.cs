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

using System.Net;
using SimpleIdServer.Scim.Common.DTOs;

namespace SimpleIdServer.Scim.Core.Factories
{
    public interface IErrorResponseFactory
    {
        ErrorResponse CreateError(string detail, HttpStatusCode status);
        ErrorResponse CreateError(string detail, HttpStatusCode status, string scimType);
    }

    internal class ErrorResponseFactory : IErrorResponseFactory
    {
        public ErrorResponse CreateError(string detail, HttpStatusCode status)
        {
            return new ErrorResponse
            {
                Detail = detail,
                Schemas = new [] { Common.Constants.Messages.Error },
                Status = (int)status
            };
        }

        public ErrorResponse CreateError(string detail, HttpStatusCode status, string scimType)
        {
            return new EnrichedErrorResponse
            {
                Detail = detail,
                Schemas = new[] { Common.Constants.Messages.Error },
                Status = (int)status,
                ScimType = scimType
            };
        }
    }
}
