using Moq;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Store.Tests
{
    public class DefaultTokenStoreTests
    {
        private Mock<IStorage> _stubStorage;
        private ITokenStore _tokenStore;

        [Fact]
        public async Task When_No_Access_Token_Exists_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _stubStorage.Setup(s => s.TryGetValueAsync<List<GrantedToken>>("granted_tokens")).Returns(Task.FromResult((List<GrantedToken>)null));

            // ACT
            var result = await _tokenStore.GetToken("scope", "clientid", null, null).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_No_Access_Token_For_Given_Scopes_And_ClientId_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedTokens = new List<GrantedToken>
            {
                new GrantedToken
                {
                    ClientId = "client",
                    Scope = "scope"
                }
            };
            _stubStorage.Setup(s => s.TryGetValueAsync<List<GrantedToken>>("granted_tokens")).Returns(Task.FromResult(grantedTokens));

            // ACT
            var firstResult = await _tokenStore.GetToken("invalid_scope", "invalid_client", null, null).ConfigureAwait(false);
            var secondResult = await _tokenStore.GetToken("scope", "invalid_client", null, null).ConfigureAwait(false);
            var thirdResult = await _tokenStore.GetToken("invalid_scope", "client", null, null).ConfigureAwait(false);

            // ASSERT
            Assert.Null(firstResult);
            Assert.Null(secondResult);
            Assert.Null(thirdResult);
        }

        [Fact]
        public async Task When_No_Common_Claims_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedTokens = new List<GrantedToken>
            {
                new GrantedToken
                {
                    ClientId = "client",
                    Scope = "scope",
                    IdTokenPayLoad = new Core.Common.JwsPayload
                    {
                        { "sub", "sub" },
                        { "address", "adr" },
                        { "email", "email" },
                        { "nonce", "nonce" }
                    }
                }
            };
            _stubStorage.Setup(s => s.TryGetValueAsync<List<GrantedToken>>("granted_tokens")).Returns(Task.FromResult(grantedTokens));

            // ACT
            var firstResult = await _tokenStore.GetToken("scope", "client",  new Core.Common.JwsPayload
            {
                { "sub", "invalid_sub" },
                { "address", "adr" },
                { "email", "email" }
            }, null).ConfigureAwait(false);
            var secondResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
                { "address", "invalid_adr" },
                { "email", "email" }
            }, null).ConfigureAwait(false);
            var thirdResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
                { "address", "adr" },
                { "email", "invalid_email" }
            }, null).ConfigureAwait(false);
            var fourthResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
                { "address", "adr" },
                { "email", "email" },
                { "nonce", "invalid_nonce" }
            }, null).ConfigureAwait(false);

            // ASSERT
            Assert.Null(firstResult);
            Assert.Null(secondResult);
            Assert.Null(thirdResult);
            Assert.Null(fourthResult);
        }

        [Fact]
        public async Task When_Common_Claims_Then_GrantedToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedTokens = new List<GrantedToken>
            {
                new GrantedToken
                {
                    ClientId = "client",
                    Scope = "scope",
                    IdTokenPayLoad = new Core.Common.JwsPayload
                    {
                        { "sub", "sub" },
                        { "address", "adr" },
                        { "email", "email" },
                        { "nonce", "nonce" }
                    }
                }
            };
            _stubStorage.Setup(s => s.TryGetValueAsync<List<GrantedToken>>("granted_tokens")).Returns(Task.FromResult(grantedTokens));

            // ACT
            var firstResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
            }, null).ConfigureAwait(false);
            var secondResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
                { "address", "adr" }
            }, null).ConfigureAwait(false);
            var fourthResult = await _tokenStore.GetToken("scope", "client", new Core.Common.JwsPayload
            {
                { "sub", "sub" },
                { "address", "adr" },
                { "nonce", "nonce" }
            }, null).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotNull(fourthResult);
        }

        private void InitializeFakeObjects()
        {
            _stubStorage = new Mock<IStorage>();
            _tokenStore = new DefaultTokenStore(_stubStorage.Object);
        }
    }
}
