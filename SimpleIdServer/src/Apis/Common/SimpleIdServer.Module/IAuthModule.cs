namespace SimpleIdServer.Module
{
    public class RedirectUrl
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string Area { get; set; }
    }

    public interface IAuthModule
    {
        string Name { get; }
        string DisplayName { get; }
        bool IsEditCredentialsEnabled { get; }
        RedirectUrl ConfigurationUrl { get; }
        RedirectUrl EditCredentialUrl { get; }
    }
}
