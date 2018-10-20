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

namespace SimpleIdentityServer.Uma.Core.Errors
{
    internal static class ErrorCodes
    {
        public const string InvalidRequestCode = "invalid_request";
        public const string InternalError = "internal_error";
        public const string InvalidResourceSetId = "invalid_resource_set_id";
        public const string InvalidId = "invalid_id";
        public const string InvalidScope = "invalid_scope";
        public const string InvalidTicket = "invalid_ticket";
        public const string ExpiredTicket = "expired_ticket";
        public const string InvalidRpt = "invalid_rpt";
        public const string InvalidGrant = "invalid_grant";
        public const string InvalidClient = "invalid_client";
    }
}