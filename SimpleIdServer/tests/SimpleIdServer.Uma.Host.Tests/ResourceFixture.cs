using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Uma.Client.Configuration;
using SimpleIdServer.Uma.Client.ResourceSet;
using SimpleIdServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Host.Tests
{
    public class ResourceFixture : IClassFixture<TestUmaServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public ResourceFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Add

        [Fact]
        public async Task When_Add_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = string.Empty
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter name needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name"
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                },
                IconUri = "invalid"
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the url invalid is not well formed", resource.Error.ErrorDescription);
        }

        #endregion

        #region Get

        [Fact]
        public async Task When_Get_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.GetByResolution("unknown",
                baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.DeleteByResolution("unknown",
                baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Update_Resource_And_No_Id_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet(), baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter id needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = "invalid",
                Name = string.Empty
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter name needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = "invalid",
                Name = "name"
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = "invalid",
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                },
                IconUri = "invalid"
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the url invalid is not well formed", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = "invalid",
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy path

        #region Get all

        [Fact]
        public async Task When_Getting_Resources_Then_Identifiers_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "token");

            // ASSERT
            Assert.NotNull(resources.Content);
            Assert.True(resources.Content.Any());
        }

        #endregion

        #region Get

        [Fact]
        public async Task When_Getting_ResourceInformation_Then_Dto_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "header");
            var resource = await _resourceSetClient.GetByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Deleting_ResourceInformation_Then_It_Doesnt_Exist()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "header");
            var resource = await _resourceSetClient.DeleteByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header");
            var information = await _resourceSetClient.GetByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.False(resource.ContainsError);
            Assert.True(information.ContainsError);
            Assert.NotNull(information);
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Adding_Resource_Then_Information_Can_Be_Retrieved()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                },
                Owner = "owner"
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
        }

        #endregion

        #region Search

        [Fact]
        public async Task When_Search_Resources_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.ResolveSearch(baseUrl + "/.well-known/uma2-configuration", new SearchResourceSet
            {
                StartIndex = 0,
                TotalResults = 100
            },
            "header");

            // ASSERTS
            Assert.NotNull(resource);
            Assert.False(resource.ContainsError);
            Assert.True(resource.Content.Content.Any());
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Updating_Resource_Then_Changes_Are_Persisted()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var updateResult = await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = resource.Content.Id,
                Name = "name2",
                Type = "type",
                Scopes = new List<string>
                {
                    "scope2"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var information = await _resourceSetClient.GetByResolution(updateResult.Content.Id, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(information);
            Assert.True(information.Content.Name == "name2");
            Assert.True(information.Content.Type == "type");
            Assert.True(information.Content.Scopes.Count() == 1 && information.Content.Scopes.First() == "scope2");
        }

        #endregion

        #region End to end
        
        [Fact]
        public async Task When_Execute_All_Operations_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                },
                IconUri = "http://localhost/picture.png",
                Type = "type",
                Uri = "http://localhost/r",
                Owner = "owner"
            },
            baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var firstResult = await _resourceSetClient.GetByResolution(resource.Content.Id, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            await _resourceSetClient.UpdateByResolution(new PutResourceSet
            {
                Id = resource.Content.Id,
                IconUri = "http://localhost/picture2.png",
                Name = "name2",
                Owner = "owner2",
                Scopes = new List<string>
                {
                    "scope2"
                },
                Type = "type2",
                Uri = "http://localhost/r2"
            }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var secondResult = await _resourceSetClient.GetByResolution(resource.Content.Id, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.Equal("name", firstResult.Content.Name);
            Assert.Equal("http://localhost/picture.png", firstResult.Content.IconUri);
            Assert.Equal("type", firstResult.Content.Type);
            Assert.Equal("http://localhost/r", firstResult.Content.Uri);
            Assert.Equal("owner", firstResult.Content.Owner);
            Assert.True(firstResult.Content.Scopes.Contains("scope"));
            Assert.Equal("name2", secondResult.Content.Name);
            Assert.Equal("http://localhost/picture2.png", secondResult.Content.IconUri);
            Assert.Equal("type2", secondResult.Content.Type);
            Assert.Equal("http://localhost/r2", secondResult.Content.Uri);
            Assert.Equal("owner2", secondResult.Content.Owner);
            Assert.True(secondResult.Content.Scopes.Contains("scope2"));
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchResourcesOperation(_httpClientFactoryStub.Object));
        }
    }
}
