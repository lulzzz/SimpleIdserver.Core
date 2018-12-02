namespace SimpleIdServer.Uma.EF.Models
{
    public class ResourceSetPolicy
    {
        public string ResourceSetId { get; set; }
        public string PolicyId { get; set; }
        public virtual ResourceSet ResourceSets { get; set; }
        public virtual Policy Policy { get; set; }
    }
}
