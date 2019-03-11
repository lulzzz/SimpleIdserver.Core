namespace SimpleIdServer.EF.Models
{
    public class ConsentClaim
    {
        public string ConsentId { get; set; }
        public string ClaimCode { get; set; }
        public Claim Claim { get; set; }
        public Consent Consent { get; set; }
    }
}
