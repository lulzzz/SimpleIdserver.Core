using System;

namespace SimpleIdServer.EF.Models
{
    public class Profile
    {
        public string Subject { get; set; }
        public string Issuer { get; set; }
        public string UserId { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
