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

using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Requests
{
    public enum GrantTypes
    {
        password,
        client_credentials,
        authorization_code,
        validate_bearer,
        refresh_token,
        uma_ticket
    }

    [DataContract]
    public class TokenRequest
    {
        [DataMember(Name = Constants.RequestTokenNames.GrantType)]
        public GrantTypes? GrantType { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.Username)]
        public string Username { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.Password)]
        public string Password { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.Scope)]
        public string Scope { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.Code)]
        public string Code { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.RedirectUri)]
        public string RedirectUri { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.RefreshToken)]
        public string RefreshToken { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.CodeVerifier)]
        public string CodeVerifier { get; set; }
        [DataMember(Name = Constants.RequestTokenNames.AmrValues)]
        public string AmrValues { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientSecret)]
        public string ClientSecret { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientAssertionType)]
        public string ClientAssertionType { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientAssertion)]
        public string ClientAssertion { get; set; }
        [DataMember(Name = Constants.RequestTokenUma.Ticket)]
        public string Ticket { get; set; }
        [DataMember(Name = Constants.RequestTokenUma.ClaimToken)]
        public string ClaimToken { get; set; }
        [DataMember(Name = Constants.RequestTokenUma.ClaimTokenFormat)]
        public string ClaimTokenFormat { get; set; }
        [DataMember(Name = Constants.RequestTokenUma.Pct)]
        public string Pct { get; set; }
        [DataMember(Name = Constants.RequestTokenUma.Rpt)]
        public string Rpt { get; set; }
    }
}