namespace SimpleIdServer.IdentityStore.EF.Models
{
    public class UserClaim
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ClaimCode { get; set; }
        public string Value { get; set; }
        public User User { get; set; }
    }
}
