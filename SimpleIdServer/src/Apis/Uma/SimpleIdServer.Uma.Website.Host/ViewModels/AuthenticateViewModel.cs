namespace SimpleIdServer.Uma.Website.Host.ViewModels
{
    public class AuthenticateViewModel
    {
        public AuthenticateViewModel(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }

        public string RedirectUrl { get; set; }
    }
}
