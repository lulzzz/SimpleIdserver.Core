﻿using SimpleIdServer.Client;
using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Selectors;
using System;

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
            return new JwksClient(new GetJsonWebKeysOperation(FakeHttpClientFactory.Instance),
                new GetDiscoveryOperation(FakeHttpClientFactory.Instance));
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
}
