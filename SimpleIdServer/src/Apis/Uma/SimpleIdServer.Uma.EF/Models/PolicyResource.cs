namespace SimpleIdServer.Uma.EF.Models
{
    public class PolicyResource
    {
        public string PolicyId { get; set; }
        public string ResourceSetId { get; set; }
        public virtual Policy Policy { get; set; }
        public virtual ResourceSet ResourceSet { get; set; }
    }
}
