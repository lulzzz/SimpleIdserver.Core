using System.Runtime.Serialization;

namespace SimpleIdServer.Authenticate.LoginPassword
{
    [DataContract]
    public class PwdCredentialOptions
    {
        [DataMember(Name = "is_regex_enabled")]
        public bool IsRegexEnabled { get; set; }
        [DataMember(Name = "regex")]
        public string RegularExpression { get; set; }
        [DataMember(Name = "pwd")]
        public string PasswordDescription { get; set; }
    }
}
