using SimpleIdServer.Bus;

namespace SimpleIdServer.Host.Tests.Services
{
    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
