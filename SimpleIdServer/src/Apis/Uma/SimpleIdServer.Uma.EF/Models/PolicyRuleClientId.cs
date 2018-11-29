namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyRuleClientId
    {
        public string PolicyRuleId { get; set; }
        public string ClientId { get; set; }
        public virtual PolicyRule PolicyRule { get; set; }
    }
}
