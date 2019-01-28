namespace SimpleIdServer.EF.Models
{
    public class DefaultSettings
    {
        public string Id { get; set; }
        public string DefaultLanguage { get; set; }
        public double DefaultAuthorizationCodeValidityPeriodInSeconds { get; set; }
        public double DefaultTokenValidityPeriodInSeconds { get; set; }
        public string DefaultTwoFactorAuthentication { get; set; }
    }
}
