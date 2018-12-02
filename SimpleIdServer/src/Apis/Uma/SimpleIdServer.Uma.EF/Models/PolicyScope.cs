namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyScope
    {
        public string PolicyId { get; set; }
        public string Scope { get; set; }
        public virtual Policy Policy { get; set; }
    }
}
