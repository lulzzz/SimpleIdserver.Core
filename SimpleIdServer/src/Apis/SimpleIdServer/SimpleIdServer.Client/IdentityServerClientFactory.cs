#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Selectors;
using SimpleIdServer.Common.Client.Factories;

namespace SimpleIdServer.Client
{
    public interface IIdentityServerClientFactory
    {
        IDiscoveryClient CreateDiscoveryClient();
        IClientAuthSelector CreateAuthSelector();
        IJwksClient CreateJwksClient();
        IUserInfoClient CreateUserInfoClient();
        IRegistrationClient CreateRegistrationClient();
        IAuthorizationClient CreateAuthorizationClient();
        IIntrospectClient CreateIntrospectionClient();
    }

    public class IdentityServerClientFactory : IIdentityServerClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public IdentityServerClientFactory()
        {
            _httpClientFactory = new HttpClientFactory();
        }

        public IdentityServerClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        public IIntrospectClient CreateIntrospectionClient()
        {
            return new IntrospectClient(new Builders.RequestBuilder(), new IntrospectOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory));
        }

        /// <summary>
        /// Get the discovery client
        /// </summary>
        /// <returns>Discovery client</returns>
        public IDiscoveryClient CreateDiscoveryClient()
        {
            return new DiscoveryClient(new GetDiscoveryOperation(_httpClientFactory));
        }

        /// <summary>
        /// Create auth client selector
        /// </summary>
        /// <returns>Choose your client authentication method</returns>
        public IClientAuthSelector CreateAuthSelector()
        {
            return new ClientAuthSelector(new TokenClientFactory(new PostTokenOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory)),
                new IntrospectClientFactory(new IntrospectOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory)),
                new RevokeTokenClientFactory(new RevokeTokenOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory)));
        }

        /// <summary>
        /// Create token client
        /// </summary>
        /// <returns>Jwks client</returns>
        public IJwksClient CreateJwksClient()
        {
            return new JwksClient(new GetJsonWebKeysOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory));
        }

        /// <summary>
        /// Create user information client
        /// </summary>
        /// <returns></returns>
        public IUserInfoClient CreateUserInfoClient()
        {
            return new UserInfoClient(new GetUserInfoOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory));
        }

        /// <summary>
        /// Creates a registration client.
        /// </summary>
        /// <returns></returns>
        public IRegistrationClient CreateRegistrationClient()
        {
            return new RegistrationClient(new RegisterClientOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory));
        }

        /// <summary>
        /// Create authorization client.
        /// </summary>
        /// <returns></returns>
        public IAuthorizationClient CreateAuthorizationClient()
        {
            return new AuthorizationClient(new GetAuthorizationOperation(_httpClientFactory), new GetDiscoveryOperation(_httpClientFactory));
        }
    }
}
