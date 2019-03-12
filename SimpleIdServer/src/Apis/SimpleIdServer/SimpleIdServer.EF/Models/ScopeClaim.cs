namespace SimpleIdServer.EF.Models
{
    public class ScopeClaim
    {
        public string ScopeName { get; set; }
        public string ClaimCode { get; set; }
        public Scope Scope { get; set; }
        public Claim Claim { get; set; }
    }
}
