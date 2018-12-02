namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyClaim
    {
        public string PolicyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public virtual Policy Policy { get; set; }
    }
}
