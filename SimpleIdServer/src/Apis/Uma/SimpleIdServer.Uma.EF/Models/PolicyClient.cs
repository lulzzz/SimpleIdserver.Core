namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyClient
    {
        public string PolicyId { get; set; }
        public string ClientId { get; set; }
        public virtual Policy Policy { get; set; }
    }
}
