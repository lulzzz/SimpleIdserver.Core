using System.ComponentModel.DataAnnotations;

namespace SimpleIdServer.Authenticate.SMS.ViewModels
{
    public class LocalAuthenticationViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
