namespace SimpleIdServer.EF.Models
{
    public sealed class Translation
    {
        public string Code { get; set; }
        public string LanguageTag { get; set; }
        public string Value { get; set; }
    }
}
