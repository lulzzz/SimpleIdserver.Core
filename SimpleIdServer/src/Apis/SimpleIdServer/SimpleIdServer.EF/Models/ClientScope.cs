namespace SimpleIdServer.EF.Models
{
    public class ClientScope
    {
        public string ClientId { get; set; }
        public string ScopeName { get; set; }
        public Scope Scope { get; set; }
        public Client Client { get; set; }
    }
}
