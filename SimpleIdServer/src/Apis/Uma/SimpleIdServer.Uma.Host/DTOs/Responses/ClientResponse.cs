﻿using SimpleIdServer.Dtos.Requests;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Host.DTOs.Responses
{
    public class ClientResponse
    {
        public string[] redirect_uris { get; set; }
        public string[] response_types { get; set; }
        public string[] grant_types { get; set; }
        public string application_type { get; set; }
        public string[] contacts { get; set; }
        public string client_name { get; set; }
        public string logo_uri { get; set; }
        public string client_uri { get; set; }
        public string policy_uri { get; set; }
        public string tos_uri { get; set; }
        public string jwks_uri { get; set; }
        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        public JsonWebKeySet jwks { get; set; }
        public string sector_identifier_uri { get; set; }
        public string subject_type { get; set; }
        public string id_token_signed_response_alg { get; set; }
        public string id_token_encrypted_response_alg { get; set; }
        public string id_token_encrypted_response_enc { get; set; }
        public string userinfo_signed_response_alg { get; set; }
        public string userinfo_encrypted_response_alg { get; set; }
        public string userinfo_encrypted_response_enc { get; set; }
        public string request_object_signing_alg { get; set; }
        public string request_object_encryption_alg { get; set; }
        public string request_object_encryption_enc { get; set; }
        public string token_endpoint_auth_method { get; set; }
        public string token_endpoint_auth_signing_alg { get; set; }
        public double default_max_age { get; set; }
        public bool require_auth_time { get; set; }
        public string default_acr_values { get; set; }
        public string initiate_login_uri { get; set; }
        public List<string> request_uris { get; set; }
        public bool scim_profile { get; set; }
    }
}
