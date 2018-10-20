using SimpleIdServer.Bus;

namespace SimpleIdServer.Uma.Host.Tests.Services
{
    internal sealed class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
