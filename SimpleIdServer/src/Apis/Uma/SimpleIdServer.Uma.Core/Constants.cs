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

namespace SimpleIdServer.Uma.Core
{
    internal static class Constants
    {
        public static string IdTokenType = "http://openid.net/specs/openid-connect-core-1_0.html#IDToken";

        public static class RptClaims
        {
            public const string Ticket = "ticket";
            public const string Scopes = "scopes";
            public const string ResourceSetId = "resource_id";
        }

        public static class AddPermissionNames
        {
            public const string ResourceSetId = "resource_set_id";
            public const string Scopes = "scopes";
        }

        public static class AddPolicyParameterNames
        {
            public const string ResourceSetIds = "resource_set_ids";
            public const string Rules = "rules";
        }

        public static class AddResourceSetParameterNames
        {
            public const string PolicyId = "_id";
            public const string ResourceSet = "resources";
        }

        public static class AddPolicyRuleParameterNames
        {
            public const string Script = "script";
            public const string Scopes = "scopes";
            public const string ClientIdsAllowed = "allowed_clients";
        }

        public static class ErrorDetailNames
        {
            public const string RequestingPartyClaims = "requesting_party_claims";
            public const string RequiredClaims = "required_claims";
            public const string ClaimName = "name";
            public const string ClaimFriendlyName = "friendly_name";
            public const string ClaimIssuer = "issuer";
            public const string RedirectUser = "redirect_user";
        }

        public static class LanguageCodes
        {
            public const string Csharp = "csharp";
        }
    }
}
