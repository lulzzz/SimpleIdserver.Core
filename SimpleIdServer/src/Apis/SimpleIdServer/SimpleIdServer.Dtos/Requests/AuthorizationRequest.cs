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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Requests
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseModes
    {
        [EnumMember(Value = Constants.ResponseModeNames.None)]
        None,
        [EnumMember(Value = Constants.ResponseModeNames.Query)]
        Query,
        [EnumMember(Value = Constants.ResponseModeNames.Fragment)]
        Fragment,
        [EnumMember(Value = Constants.ResponseModeNames.FormPost)]
        FormPost
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseTypes
    {
        [EnumMember(Value = Constants.ResponseTypeNames.Code)]
        Code,
        [EnumMember(Value = Constants.ResponseTypeNames.Token)]
        Token,
        [EnumMember(Value = Constants.ResponseTypeNames.IdToken)]
        IdToken
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayModes
    {
        [EnumMember(Value = Constants.PageNames.Page)]
        Page,
        [EnumMember(Value = Constants.PageNames.Popup)]
        Popup,
        [EnumMember(Value = Constants.PageNames.Touch)]
        Touch,
        [EnumMember(Value = Constants.PageNames.Wap)]
        Wap
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CodeChallengeMethods
    {
        [EnumMember(Value = Constants.CodeChallenges.Plain)]
        Plain,
        [EnumMember(Value = Constants.CodeChallenges.S256)]
        S256
    }

    [DataContract]
    public class AuthorizationRequest
    {
        private Dictionary<ResponseTypes, string> _mappingResponseTypesToNames = new Dictionary<ResponseTypes, string>
        {
            { ResponseTypes.Code, Constants.ResponseTypeNames.Code },
            { ResponseTypes.Token, Constants.ResponseTypeNames.Token },
            { ResponseTypes.IdToken, Constants.ResponseTypeNames.IdToken }
        };

        public AuthorizationRequest() { }

        public AuthorizationRequest(IEnumerable<string> scopes, IEnumerable<ResponseTypes> responseTypes, string clientId, string redirectUri, string state)
        {
            Scope = string.Join(" ", scopes);
            ResponseType = string.Join(" ", responseTypes.Select(s => _mappingResponseTypesToNames[s]));
            ClientId = clientId;
            RedirectUri = redirectUri;
            State = state;
        }

        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Scope)]
        public string Scope { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.ResponseType)]
        public string ResponseType { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.RedirectUri)]
        public string RedirectUri { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.State)]
        public string State { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.ResponseMode)]
        public ResponseModes? ResponseMode { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Nonce)]
        public string Nonce { get; set; }
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Display)]
        public DisplayModes? Display { get; set; }
        /// <summary>
        /// The possible values are : none, login, consent, select_account
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Prompt)]
        public string Prompt { get; set; }
        /// <summary>
        /// Maximum authentication age.
        /// Specifies allowable elapsed time in seconds since the last time the end-user
        ///  was actively authenticated by the OP.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.MaxAge)]
        public double MaxAge { get; set; }
        /// <summary>
        /// End-User's preferred languages
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.UiLocales)]
        public string UiLocales { get; set; }
        /// <summary>
        /// Token previousely issued by the Authorization Server.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.IdTokenHint)]
        public string IdTokenHint { get; set; }
        /// <summary>
        /// Hint to the authorization server about the login identifier the end-user might use to log in.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.LoginHint)]
        public string LoginHint { get; set; }
        /// <summary>
        /// Request that specific Claims be returned from the UserInfo endpoint and/or in the id token.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Claims)]
        public string Claims { get; set; }
        /// <summary>
        /// Requested Authentication Context Class References values.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.AcrValues)]
        public string AcrValues { get; set; }
        /// <summary>
        /// Self-contained parameter and can be optionally be signed and / or encrypted
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.Request)]
        public string Request { get; set; }
        /// <summary>
        /// Enables OpenID connect requests to be passed by reference rather than by value.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.RequestUri)]
        public string RequestUri { get; set; }
        /// <summary>
        /// Code challenge.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.CodeChallenge)]
        public string CodeChallenge { get; set; }
        /// <summary>
        /// Code challenge method.
        /// </summary>
        [DataMember(Name = Constants.RequestAuthorizationCodeNames.CodeChallengeMethod)]
        public CodeChallengeMethods? CodeChallengeMethod { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = Constants.EventResponseNames.AggregateId)]
        public string ProcessId { get; set; }
        [DataMember(Name = "origin_url")]
        public string OriginUrl { get; set; }
        [DataMember(Name = "session_id")]
        public string SessionId { get; set; }
        [DataMember(Name = "amr_values")]
        public string AmrValues { get; set; }
    }
}
