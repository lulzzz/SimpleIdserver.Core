using System.Runtime.Serialization;

namespace SimpleIdServer.Authenticate.LoginPassword.DTOs
{
    [DataContract]
    public class EditLoginPasswordRequest
    {
        [DataMember(Name = "actual_password")]
        public string ActualPassword { get; set; }
        [DataMember(Name = "new_password")]
        public string NewPassword { get; set; }
        [DataMember(Name = "subject")]
        public string Subject { get; set; }
    }
}