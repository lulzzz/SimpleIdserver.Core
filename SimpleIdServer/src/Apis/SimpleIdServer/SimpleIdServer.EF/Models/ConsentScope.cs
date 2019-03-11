namespace SimpleIdServer.EF.Models
{
    public class ConsentScope
    {
        public string ConsentId { get; set; }
        public string ScopeName { get; set; }
        public Consent Consent { get; set; }
        public Scope Scope { get; set; }
    }
}
