namespace SimpleIdServer.Core.Parameters
{
    public class UpdateUserCredentialParameter
    {
        public string UserId { get; set; }
        public string CredentialType { get; set; }
        public string NewValue { get; set; }
    }
}
