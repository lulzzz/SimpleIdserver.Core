namespace SimpleIdServer.Module
{
    public interface IUiModule
    {
        string DisplayName { get; }
        string Name { get; }
        string Picture { get; }
        RedirectUrl RedirectionUrl { get; }
    }
}
