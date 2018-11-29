namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyRuleClaim
    {
        public string PolicyRuleId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public virtual PolicyRule PolicyRule { get; set; }
    }
}
