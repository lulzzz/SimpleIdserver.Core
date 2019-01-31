using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Authenticate.Basic.DTOs
{
    [DataContract]
    public class AuthCredentialConfigurationResponse
    {
        [DataMember(Name = "is_editable")]
        public bool IsEditable { get; set; }
        [DataMember(Name = "url")]
        public string EditUrl { get; set; }
        [DataMember(Name = "fields")]
        public IEnumerable<AuthCredentialFieldResponse> Fields { get; set; }
    }

    [DataContract]
    public class AuthCredentialFieldResponse
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }

    [DataContract]
    public class AuthConfigurationResponse
    {
        [DataMember(Name = "confs")]
        public IEnumerable<string> Configurations { get; set; }
        [DataMember(Name = "credential")]
        public AuthCredentialConfigurationResponse Credential { get; set; }
    }
}
