using System.Collections.Generic;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.Core.Parameters
{
    public class RegistrationParameter
    {
        public List<string> RedirectUris { get; set; }
        public List<string> PostLogoutRedirectUris { get; set; }
        public List<Scope> Scopes { get; set; }
        public List<ResponseType> ResponseTypes { get; set; }
        public List<GrantType> GrantTypes { get; set; }
        public ApplicationTypes? ApplicationType { get; set; }
        public List<string> Contacts { get; set; }
        public string ClientName { get; set; }
        public string LogoUri { get; set; }
        public string ClientUri { get; set; }
        public string PolicyUri { get; set; }
        public string TosUri { get; set; }
        public string JwksUri { get; set; }
        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        public JsonWebKeySet Jwks { get; set; }
        public string SectorIdentifierUri { get; set; }
        public string SubjectType { get; set; }
        public string IdTokenSignedResponseAlg { get; set; }
        public string IdTokenEncryptedResponseAlg { get; set; }
        public string IdTokenEncryptedResponseEnc { get; set; }
        public string UserInfoSignedResponseAlg { get; set; }
        public string UserInfoEncryptedResponseAlg { get; set; }
        public string UserInfoEncryptedResponseEnc { get; set; }
        public string RequestObjectSigningAlg { get; set; }
        public string RequestObjectEncryptionAlg { get; set; }
        public string RequestObjectEncryptionEnc { get; set; }
        public string TokenEndPointAuthMethod { get; set; }
        public string TokenEndPointAuthSigningAlg { get; set; }
        public double DefaultMaxAge { get; set; }
        public bool RequireAuthTime { get; set; }
        public string DefaultAcrValues { get; set; }
        public string InitiateLoginUri { get; set; }
        public List<string> RequestUris { get; set; }
        public bool ScimProfile { get; set; }
        public bool RequirePkce { get; set; }
    }
}
