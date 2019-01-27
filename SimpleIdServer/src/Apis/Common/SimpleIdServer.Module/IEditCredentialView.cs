namespace SimpleIdServer.Module
{
    public class RedirectUrl
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string Area { get; set; }
    }

    public interface IEditCredentialView
    {
        string DisplayName { get; }
        RedirectUrl Href { get; }
        bool IsEnabled { get; }
    }
}
