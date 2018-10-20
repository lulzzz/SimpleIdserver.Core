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
using System.Collections.Generic;

namespace SimpleIdServer.Core.Common.Models
{
    public enum ResponseType
    {
        code = 0,
        token = 1,
        id_token = 2
    }

    public enum GrantType
    {
        authorization_code = 0,
        @implicit = 1,
        refresh_token = 2,
        client_credentials = 3,
        password = 4,
        uma_ticket = 5
    }

    public enum TokenEndPointAuthenticationMethods
    {
        // Defaut value
        client_secret_basic = 0,
        client_secret_post = 1,
        client_secret_jwt = 2,
        private_key_jwt = 3,
        tls_client_auth = 4,
        none = 5
    }

    public enum ApplicationTypes
    {
        native = 0,
        web = 1
    }

    public enum ClientSecretTypes
    {
        SharedSecret= 0,
        X509Thumbprint = 1,
        X509Name = 2
    }

    public class ClientSecret
    {
        public ClientSecretTypes Type { get; set; }
        public string Value { get; set; }
    }

    public class Client
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secrets.
        /// </summary>
        public ICollection<ClientSecret> Secrets { get; set; }

        public string ClientName { get; set; }

        public string LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the home page of the client.
        /// </summary>
        public string ClientUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
        /// </summary>
        public string PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
        /// </summary>
        public string TosUri { get; set; }

        #region Encryption mechanism for ID TOKEN

        /// <summary>
        /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
        /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
        /// </summary>
        public string IdTokenSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// The default is that no encryption is performed
        /// </summary>
        public string IdTokenEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
        /// </summary>
        public string IdTokenEncryptedResponseEnc { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the client authentication method for the Token Endpoint. 
        /// </summary>
        public TokenEndPointAuthenticationMethods TokenEndPointAuthMethod { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 response_type values
        /// </summary>
        public List<ResponseType> ResponseTypes { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 grant types
        /// </summary>
        public List<GrantType> GrantTypes { get; set; }

        /// <summary>
        /// Gets or sets a list of OAUTH2.0 grant_types.
        /// </summary>
        public List<Scope> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets an array of Redirection URI values used by the client.
        /// </summary>
        public List<string> RedirectionUrls { get; set; }

        /// <summary>
        /// Gets or sets the type of application
        /// </summary>
        public ApplicationTypes ApplicationType { get; set; }

        /// <summary>
        /// Url for the Client's JSON Web Key Set document
        /// </summary>
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the list of json web keys
        /// </summary>
        public List<JsonWebKey> JsonWebKeys { get; set; }

        /// <summary>
        /// Gets or sets the list of contacts
        /// </summary>
        public List<string> Contacts { get; set; }

        /// <summary>
        /// Get or set the sector identifier uri
        /// </summary>
        public string SectorIdentifierUri { get; set; }

        /// <summary>
        /// Gets or sets the subject type
        /// </summary>
        public string SubjectType { get; set; }

        /// <summary>
        /// Gets or sets the user info signed response algorithm
        /// </summary>
        public string UserInfoSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response algorithm
        /// </summary>
        public string UserInfoEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response enc
        /// </summary>
        public string UserInfoEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the request objects signing algorithm
        /// </summary>
        public string RequestObjectSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption algorithm
        /// </summary>
        public string RequestObjectEncryptionAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption enc
        /// </summary>
        public string RequestObjectEncryptionEnc { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing algorithm
        /// </summary>
        public string TokenEndPointAuthSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the default max age
        /// </summary>
        public double DefaultMaxAge { get; set; }

        /// <summary>
        /// Gets or sets the require authentication time
        /// </summary>
        public bool RequireAuthTime { get; set; }

        /// <summary>
        /// Gets or sets the default acr values
        /// </summary>
        public string DefaultAcrValues { get; set; }

        /// <summary>
        /// Gets or sets the initiate login uri
        /// </summary>
        public string InitiateLoginUri { get; set; }

        /// <summary>
        /// Gets or sets the list of request uris
        /// </summary>
        public List<string> RequestUris { get; set; } 

        /// <summary>
        /// Gets or sets use SCIM protocol to access user information.
        /// </summary>
        public bool ScimProfile { get; set; }

        /// <summary>
        /// Client require PKCE.
        /// </summary>
        public bool RequirePkce { get; set; }

        /// <summary>
        /// Get or sets the post logout redirect uris.
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
