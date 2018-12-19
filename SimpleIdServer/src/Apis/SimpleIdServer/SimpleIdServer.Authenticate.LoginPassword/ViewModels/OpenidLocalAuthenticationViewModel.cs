using SimpleIdServer.Authenticate.Basic.ViewModels;

namespace SimpleIdServer.Authenticate.LoginPassword.ViewModels
{
    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}