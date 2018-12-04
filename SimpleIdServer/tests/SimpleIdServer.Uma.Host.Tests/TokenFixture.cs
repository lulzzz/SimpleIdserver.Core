using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Client;
using SimpleIdServer.Client.Operations;
using SimpleIdServer.Client.Selectors;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.Jwt.Signature;
using SimpleIdServer.Uma.Client.Configuration;
using SimpleIdServer.Uma.Client.Permission;
using SimpleIdServer.Uma.Client.Policy;
using SimpleIdServer.Uma.Client.ResourceSet;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Host.Tests.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Host.Tests
{
    public class TokenFixture : IClassFixture<TestUmaServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IJwsGenerator _jwsGenerator;
        private IClientAuthSelector _clientAuthSelector;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private IPolicyClient _policyClient;
        private readonly TestUmaServerFixture _server;

        public TokenFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        #region Errors

        [Fact]
        public async Task When_Ticket_Id_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var token = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Try to get the access token via "ticket_id" grant-type.
                .UseTicketId("ticket_id", "")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERT
            Assert.NotNull(token);
            Assert.True(token.ContainsError);
            Assert.Equal("invalid_ticket", token.Error.Error);
            Assert.Equal("the ticket ticket_id doesn't exist", token.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_And_Client_Is_Not_Correct_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            var jwsPayload = new JwsPayload();
            jwsPayload.Add("iss", "http://server.example.com");
            jwsPayload.Add("sub", "248289761001");
            jwsPayload.Add("aud", "s6BhdRkqt3");
            jwsPayload.Add("nonce", "n-0S6_WzA2Mj");
            jwsPayload.Add("exp", "1311281970");
            jwsPayload.Add("iat", "1311280970");
            var jwt = _jwsGenerator.Generate(jwsPayload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Get PAT.
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");
            // UserStore.Instance().ClientId = "resource_server";
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "read",
                    "write",
                    "execute"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            {

                IsResourceOwnerConsentNeeded = false,
                Scopes = new List<string>
                {
                    "read"
                },
                ClientIdsAllowed = new List<string>
                {
                    "resource_server"
                },
                Claims = new List<PostClaim>
                {
                    new PostClaim { Type = "sub", Value = "248289761001" }
                },
                ResourceSetIds = new List<string>
                {
                    resource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            UserStore.Instance().ClientId = "invalid_client";
            var ticket = await _permissionClient.AddByResolution(new PostPermission // Add permission & retrieve a ticket id.
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            UserStore.Instance().ClientId = "resource_server";
            var token = await _clientAuthSelector.UseNoAuthentication() // Try to get the access token via "ticket_id" grant-type.
                .UseTicketId(ticket.Content.TicketId, jwt)
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS.
            Assert.True(token.ContainsError);
            Assert.Equal("not_authorized", token.Error.Error);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_And_UserRole_Is_Not_Correct_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            var jwsPayload = new JwsPayload();
            jwsPayload.Add("iss", "http://server.example.com");
            jwsPayload.Add("sub", "248289761001");
            jwsPayload.Add("role", new[] { "user", "user1" });
            jwsPayload.Add("aud", "s6BhdRkqt3");
            jwsPayload.Add("nonce", "n-0S6_WzA2Mj");
            jwsPayload.Add("exp", "1311281970");
            jwsPayload.Add("iat", "1311280970");
            var jwt = _jwsGenerator.Generate(jwsPayload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Get PAT.
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");
            // UserStore.Instance().ClientId = "resource_server";
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "read",
                    "write",
                    "execute"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            {

                IsResourceOwnerConsentNeeded = false,
                Scopes = new List<string>
                {
                    "read"
                },
                ClientIdsAllowed = new List<string>
                {
                    "resource_server"
                },
                Claims = new List<PostClaim>
                {
                    new PostClaim { Type = "sub", Value = "248289761001" },
                    new PostClaim { Type = "role", Value = "administrator" }
                },
                ResourceSetIds = new List<string>
                {
                    resource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var ticket = await _permissionClient.AddByResolution(new PostPermission // Add permission & retrieve a ticket id.
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var token = await _clientAuthSelector.UseNoAuthentication() // Try to get the access token via "ticket_id" grant-type.
                .UseTicketId(ticket.Content.TicketId, jwt)
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS.
            Assert.True(token.ContainsError);
            Assert.Equal("not_authorized", token.Error.Error);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_And_Require_ResourceOwnerConsent_Then_RequestSubmitted()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            var jwsPayload = new JwsPayload();
            jwsPayload.Add("iss", "http://server.example.com");
            jwsPayload.Add("sub", "248289761001");
            jwsPayload.Add("role", new[] { "bad_role" });
            jwsPayload.Add("aud", "s6BhdRkqt3");
            jwsPayload.Add("nonce", "n-0S6_WzA2Mj");
            jwsPayload.Add("exp", "1311281970");
            jwsPayload.Add("iat", "1311280970");
            var jwt = _jwsGenerator.Generate(jwsPayload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Get PAT.
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "read",
                    "write",
                    "execute"
                },
                AcceptPendingRequest = true
            },
            baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            {
                Scopes = new List<string>
                {
                    "read"
                },
                ClientIdsAllowed = new List<string>
                {
                    "resource_server"
                },
                Claims = new List<PostClaim>
                {
                    new PostClaim { Type = "sub", Value = "248289761001" },
                    new PostClaim { Type = "role", Value = "administrator" }
                },
                ResourceSetIds = new List<string>
                {
                    resource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var ticket = await _permissionClient.AddByResolution(new PostPermission // Add permission & retrieve a ticket id.
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var token = await _clientAuthSelector.UseNoAuthentication()
                .UseTicketId(ticket.Content.TicketId, jwt)
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration").ConfigureAwait(false);

            // ASSERTS.
            Assert.True(token.ContainsError);
            Assert.Equal("not_authorized", token.Error.Error);
            var detail = JObject.FromObject(token.Error.ErrorDetails.First());
            Assert.NotNull(detail);
            Assert.Equal("RequestSubmitted", detail["status"].ToString());
            Assert.Equal("a request has been submitted", detail["details"].ToString());
        }
        
        #endregion

        #region Happy path

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server")
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_Then_AccessToken_Is_Returned()
        { 
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            var sub = Guid.NewGuid().ToString();
            var jwsPayload = new JwsPayload();
            jwsPayload.Add("iss", "http://server.example.com");
            jwsPayload.Add("sub", sub);
            jwsPayload.Add("aud", "s6BhdRkqt3");
            jwsPayload.Add("nonce", "n-0S6_WzA2Mj");
            jwsPayload.Add("exp", "1311281970");
            jwsPayload.Add("iat", "1311280970");
            var jwt = _jwsGenerator.Generate(jwsPayload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Get PAT.
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "read",
                    "write",
                    "execute"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            {

                IsResourceOwnerConsentNeeded = false,
                Scopes = new List<string>
                {
                    "read"
                },
                ClientIdsAllowed = new List<string>
                {
                    "resource_server"
                },
                Claims = new List<PostClaim>
                {
                    new PostClaim { Type = "sub", Value = sub }
                },
                ResourceSetIds = new List<string>
                {
                    resource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", result.Content.AccessToken);
            var ticket = await _permissionClient.AddByResolution(new PostPermission // Add permission & retrieve a ticket id.
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var token = await _clientAuthSelector.UseNoAuthentication() // Try to get the access token via "ticket_id" grant-type.
                .UseTicketId(ticket.Content.TicketId, jwt)
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS.
            Assert.NotNull(token);            
        }

        #endregion

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = provider.GetService<IJwsGenerator>();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchResourcesOperation(_httpClientFactoryStub.Object));
            _permissionClient = new PermissionClient(
                new AddPermissionsOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _policyClient = new PolicyClient(new AddPolicyOperation(_httpClientFactoryStub.Object),
                new GetPolicyOperation(_httpClientFactoryStub.Object),
                new DeletePolicyOperation(_httpClientFactoryStub.Object),
                new GetPoliciesOperation(_httpClientFactoryStub.Object),
                new AddResourceToPolicyOperation(_httpClientFactoryStub.Object),
                new DeleteResourceFromPolicyOperation(_httpClientFactoryStub.Object),
                new UpdatePolicyOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchPoliciesOperation(_httpClientFactoryStub.Object));
        }
    }
}
