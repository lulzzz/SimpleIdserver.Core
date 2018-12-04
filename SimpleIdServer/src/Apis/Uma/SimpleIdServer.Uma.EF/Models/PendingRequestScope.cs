namespace SimpleIdServer.Uma.EF.Models
{
    public class PendingRequestScope
    {
        public string Scope { get; set; }
        public string PendingRequestId { get; set; }
        public virtual ResourcePendingRequest PendingRequest { get; set; }
    }
}
