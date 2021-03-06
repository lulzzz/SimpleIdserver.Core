﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Uma.Client.Configuration;
using SimpleIdServer.Uma.Client.Permission;
using SimpleIdServer.Uma.Client.Policy;
using SimpleIdServer.Uma.Client.ResourceSet;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Host.Tests.MiddleWares;
using Xunit;

namespace SimpleIdServer.Uma.Host.Tests
{
    public class PermissionFixture : IClassFixture<TestUmaServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private readonly TestUmaServerFixture _server;

        public PermissionFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        #region Errors

        [Fact]
        public async Task When_ResourceSetId_Is_Null_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = string.Empty
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_request", ticket.Error.Error);
            Assert.Equal("the parameter resource_set_id needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Is_Null_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = "resource"
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_request", ticket.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = "resource",
                Scopes = new List<string>
                {
                    "scope"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_resource_set_id", ticket.Error.Error);
            Assert.Equal("resource set resource doesn't exist", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "scopescopescope"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_scope", ticket.Error.Error);
            Assert.Equal("one or more scopes are not valid", ticket.Error.ErrorDescription);
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket.Content.TicketId);
        }

        [Fact]
        public async Task When_Adding_Permissions_Then_TicketIds_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var permissions = new List<PostPermission>
            {
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                },
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }
            };

            // ACT
            var ticket = await _permissionClient.AddByResolution(permissions, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(ticket);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _policyClient = new PolicyClient(new AddPolicyOperation(_httpClientFactoryStub.Object),
                new GetPolicyOperation(_httpClientFactoryStub.Object),
                new DeletePolicyOperation(_httpClientFactoryStub.Object),
                new GetPoliciesOperation(_httpClientFactoryStub.Object),
                new AddResourceToPolicyOperation(_httpClientFactoryStub.Object),
                new DeleteResourceFromPolicyOperation(_httpClientFactoryStub.Object),
                new UpdatePolicyOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchPoliciesOperation(_httpClientFactoryStub.Object));
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
        }
    }
}
