namespace SimpleIdServer.EF.Models
{
    public enum SecretTypes
    {
        SharedSecret,
        X509Thumbprint,
        X509Name
    }

    public class ClientSecret
    {
        public string Id { get; set; }
        public SecretTypes Type { get; set; }
        public string Value { get; set; }
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }
    }
}
