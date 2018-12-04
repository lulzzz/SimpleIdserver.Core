using SimpleIdServer.Client;
using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Selectors;
using SimpleIdServer.Core.Api.Jwks.Actions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Tests.Fakes
{
    public class FakeIdentityServerClientFactory : IIdentityServerClientFactory
    {
        public IAuthorizationClient CreateAuthorizationClient()
        {
            throw new NotImplementedException();
        }

        public IClientAuthSelector CreateAuthSelector()
        {
            var httpClient = FakeHttpClientFactory.Instance;
            var postTokenOperation = new PostTokenOperation(httpClient);
            var getDiscoveryOperation = new GetDiscoveryOperation(httpClient);
            var introspectionOperation = new IntrospectOperation(httpClient);
            var revokeTokenOperation = new RevokeTokenOperation(httpClient);
            return new ClientAuthSelector(
                   new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                   new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                   new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }

        public IDiscoveryClient CreateDiscoveryClient()
        {
            return new DiscoveryClient(
                new GetDiscoveryOperation(FakeHttpClientFactory.Instance));
        }

        public IIntrospectClient CreateIntrospectionClient()
        {
            return null;
        }

        public IJwksClient CreateJwksClient()
        {
            return new JwksClient(new FakeGetJsonWebKeysOperation(),
                new FakeGetDiscoveryOperation());
        }

        public IRegistrationClient CreateRegistrationClient()
        {
            return null;
        }

        public IUserInfoClient CreateUserInfoClient()
        {
            return null;
        }
    }

    public class FakeGetJsonWebKeysOperation : IGetJsonWebKeysOperation
    {
        public Task<JsonWebKeySet> ExecuteAsync(Uri jwksUri)
        {
            var sig = SharedContext.Instance.SignatureKey;
            var enricher = new JsonWebKeyEnricher();
            var record = new Dictionary<string, object>();
            var result = enricher.GetPublicKeyInformation(sig);
            result.AddRange(enricher.GetJsonWebKeyInformation(sig));
            var keys = new List<Dictionary<string, object>>();
            keys.Add(result);
            return Task.FromResult(new JsonWebKeySet
            {
                Keys = keys
            });
        }
    }

    public class FakeGetDiscoveryOperation : IGetDiscoveryOperation
    {
        public Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri)
        {
            return Task.FromResult(new DiscoveryInformation
            {
                JwksUri = "http://localhost/jwks"
            });
        }
    }
}
