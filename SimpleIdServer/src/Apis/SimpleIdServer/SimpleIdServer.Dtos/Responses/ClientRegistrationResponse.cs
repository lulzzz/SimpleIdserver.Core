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

using SimpleIdServer.Dtos.Requests;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Responses
{
    [DataContract]
    public class ClientRegistrationResponse
    {
        [DataMember(Name = Constants.ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = Constants.ClientAuthNames.ClientSecret)]
        public string ClientSecret { get; set; }

        //// TODO : RETURNS REGISTRATIONA ACCESS TOKEN & REGISTRATION CLIENT URI.
        // [DataMember(Name = StandardRegistrationResponseNames.RegistrationAccessToken)]
        // public string RegistrationAccessToken { get; set; }

        // [DataMember(Name = StandardRegistrationResponseNames.RegistrationClientUri)]
        // public string RegistrationClientUri { get; set; }

        [DataMember(Name = Constants.RegistrationResponseNames.ClientIdIssuedAt)]
        public string ClientIdIssuedAt { get; set; }
        [DataMember(Name = Constants.RegistrationResponseNames.ClientSecretExpiresAt)]
        public int ClientSecretExpiresAt { get; set; }
        [DataMember(Name = Constants.ClientNames.RedirectUris)]
        public string[] RedirectUris { get; set; }
        [DataMember(Name = Constants.ClientNames.PostLogoutRedirectUris)]
        public string[] PostLogoutRedirectUris { get; set; }
        [DataMember(Name = Constants.ClientNames.ResponseTypes)]
        public string[] ResponseTypes { get; set; }
        [DataMember(Name = Constants.ClientNames.GrantTypes)]
        public string[] GrantTypes { get; set; }
        [DataMember(Name = Constants.ClientNames.ApplicationType)]
        public string ApplicationType { get; set; }
        [DataMember(Name = Constants.ClientNames.Contacts)]
        public string[] Contacts { get; set; }
        [DataMember(Name = Constants.ClientNames.ClientName)]
        public string ClientName { get; set; }
        [DataMember(Name = Constants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }
        [DataMember(Name = Constants.ClientNames.ClientUri)]
        public string ClientUri { get; set; }
        [DataMember(Name = Constants.ClientNames.PolicyUri)]
        public string PolicyUri { get; set; }
        [DataMember(Name = Constants.ClientNames.TosUri)]
        public string TosUri { get; set; }
        [DataMember(Name = Constants.ClientNames.JwksUri)]
        public string JwksUri { get; set; }
        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [DataMember(Name = Constants.ClientNames.Jwks)]
        public JsonWebKeySet Jwks { get; set; }
        [DataMember(Name = Constants.ClientNames.SectorIdentifierUri)]
        public string SectorIdentifierUri { get; set; }
        [DataMember(Name = Constants.ClientNames.SubjectType)]
        public string SubjectType { get; set; }
        [DataMember(Name = Constants.ClientNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }
        [DataMember(Name = Constants.ClientNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }
        [DataMember(Name = Constants.ClientNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }
        [DataMember(Name = Constants.ClientNames.TokenEndpointAuthMethod)]
        public string TokenEndpointAuthMethod { get; set; }
        [DataMember(Name = Constants.ClientNames.TokenEndpointAuthSigningAlg)]
        public string TokenEndpointAuthSigningAlg { get; set; }
        [DataMember(Name = Constants.ClientNames.DefaultMaxAge)]
        public double DefaultMaxAge { get; set; }
        [DataMember(Name = Constants.ClientNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }
        [DataMember(Name = Constants.ClientNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }
        [DataMember(Name = Constants.ClientNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }
        [DataMember(Name = Constants.ClientNames.RequestUris)]
        public IEnumerable<string> RequestUris { get; set; }
        [DataMember(Name = Constants.ClientNames.ScimProfile)]
        public bool ScimProfile { get; set; }
        [DataMember(Name = Constants.ClientNames.RequirePkce)]
        public bool RequirePkce { get; set; }
    }
}
