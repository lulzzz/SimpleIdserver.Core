namespace SimpleIdServer.EF.Models
{
    public class AuthenticationContextclassReference
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string DisplayName { get; set; }
        public int Type { get; set; }
        public string AmrLst { get; set; }
    }
}