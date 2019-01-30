namespace SimpleIdServer.Core.Parameters
{
    public class AddUserCredentialParameter
    {
        public string UserId { get; set;}
        public string CredentialType { get; set; }
        public string Value { get; set; }
    }
}
