namespace SimpleIdServer.Uma.EF.Models
{
    public class ResourceScope
    {
        public string ResourceId { get; set; }
        public string Scope { get; set; }
        public virtual ResourceSet ResourceSet { get; set; }
    }
}
