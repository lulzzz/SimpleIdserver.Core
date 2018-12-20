namespace SimpleIdServer.EF.Models
{
    public class PasswordSettings
    {
        public string Id { get; set; }
        public double PasswordExpiresIn { get; set; }
        public bool IsRegexEnabled { get; set; }
        public string RegularExpression { get; set; }
        public bool IsBlockAccountPolicyEnabled { get; set; }
        public int NumberOfAuthenticationAttempts { get; set; }
        public int AuthenticationIntervalsInSeconds { get; set; }
        public string PasswordDescription { get; set; }
    }
}
