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

namespace SimpleIdentityServer.Client
{
    internal static class Constants
    {
        public static class TokenRequestNames
        {
            public const string GrantType = "grant_type";

            public const string Username = "username";

            public const string Password = "password";

            public const string Scope = "scope";

            public const string ClientId = "client_id";

            public const string ClientSecret = "client_secret";

            public const string Code = "code";

            public const string RedirectUri = "redirect_uri";

            public const string ClientAssertionType = "client_assertion_type";

            public const string ClientAssertion = "client_assertion";

            public const string RefreshToken = "refresh_token";
        }
    }
}
