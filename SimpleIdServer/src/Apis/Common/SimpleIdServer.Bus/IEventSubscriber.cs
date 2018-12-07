namespace SimpleIdServer.Bus
{
    public interface IEventSubscriber
    {
        void Listen();
        void AddHandler(IEventHandler eventHandler);
    }
}
