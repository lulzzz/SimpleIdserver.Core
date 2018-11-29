namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyRuleScope
    {
        public string PolicyRuleId { get; set; }
        public string Scope { get; set; }
        public virtual PolicyRule PolicyRule { get; set; }
    }
}
