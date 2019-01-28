namespace SimpleIdServer.EF.Models
{
    public class CredentialSetting
    {
        public string CredentialType { get; set; }
        public double ExpiresIn { get; set; }
        public string Options { get; set; }
        public bool IsBlockAccountPolicyEnabled { get; set; }
        public int NumberOfAuthenticationAttempts { get; set; }
        public int AuthenticationIntervalsInSeconds { get; set; }
    }
}