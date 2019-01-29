namespace SimpleIdServer.Shell
{
    public class ShellModuleOptions
    {
        public ShellModuleOptions()
        {
            ClientId = "adminUiModule";
            ClientSecret = "adminUiModuleSecret";
        }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}