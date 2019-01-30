using System.ComponentModel.DataAnnotations;
using SimpleIdServer.Authenticate.Basic.ViewModels;

namespace SimpleIdServer.Authenticate.SMS.ViewModels
{
    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}