using System;

namespace SimpleIdServer.Bus
{
    public interface IEventSubscriber : IDisposable
    {
        void Listen();
        void AddHandler(IEventHandler eventHandler);
    }
}
