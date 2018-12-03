using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Uma.Host.Tests.Fakes;
using System;
using System.Net.Http;

namespace SimpleIdServer.Uma.Host.Tests
{
    public class TestUmaServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }
        public SharedContext SharedCtx { get; }

        public TestUmaServerFixture()
        {
            SharedCtx = new SharedContext();
            var startup = new FakeUmaStartup(SharedCtx);
            Server = new TestServer(new WebHostBuilder()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);
                })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeUmaStartup).GetType().Assembly.FullName));
            Client = Server.CreateClient();
            FakeHttpClientFactory.Instance.Set(Server);
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
