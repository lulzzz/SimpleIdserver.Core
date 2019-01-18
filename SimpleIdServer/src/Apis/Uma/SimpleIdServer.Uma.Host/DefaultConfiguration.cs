using SimpleIdServer.Core.Common.Models;
using System;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Host
{
    internal static class DefaultConfiguration
    {
        public static List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "uma_protection",
                Description = "Access to UMA permission, resource set",
                IsOpenIdScope = false,
                IsDisplayedInConsent = false,
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "register_client",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "adminapi",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Access to the admin ui",
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            },
			new Scope
			{
				Name = "scim_manage",
				IsExposed = false,
				IsOpenIdScope = false,
				IsDisplayedInConsent = true,
				Description = "Manage the SCIM resources",
				Type = ScopeType.ProtectedApi,
				UpdateDateTime = DateTime.UtcNow,
				CreateDateTime = DateTime.UtcNow
			},
			new Scope
			{
				Name = "scim_read",
				IsExposed = false,
				IsOpenIdScope = false,
				IsDisplayedInConsent = true,
				Description = "Read the SCIM resources",
				Type = ScopeType.ProtectedApi,
				UpdateDateTime = DateTime.UtcNow,
				CreateDateTime = DateTime.UtcNow
			}		
        };

        public static List<SimpleIdServer.Core.Common.Models.Client> DEFAULT_CLIENTS = new List<SimpleIdServer.Core.Common.Models.Client>
        {
            new SimpleIdServer.Core.Common.Models.Client
            {
                ClientId = "registrationClient",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "registrationPassword"
                    }
                },
                ClientName = "Client administrator",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "register_client"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            },
            new SimpleIdServer.Core.Common.Models.Client
            {
                ClientId = "adminUiClient",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "adminUiPassword"
                    }
                },
                ClientName = "Client administrator",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "adminapi"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            },
            new SimpleIdServer.Core.Common.Models.Client
            {
                ClientId = "scimManagerClient",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "scimManagerPassword"
                    }
                },
                ClientName = "SCIM manager client",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
					new Scope
					{
						Name = "scim_manage"
					},
                    new Scope
                    {
                        Name = "scim_read"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            },
            new SimpleIdServer.Core.Common.Models.Client
            {
                ClientId = "resourceServer",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "resourceServerPassword"
                    }
                },
                ClientName = "Resource server",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "uma_protection"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            },
            new SimpleIdServer.Core.Common.Models.Client
            {
                ClientId = "jobOrchestrator",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "jobOrchestratorPassword"
                    }
                },
                ClientName = "Job orchestrator",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "uma_protection"
                    },
                    new Scope
                    {
                        Name = "adminapi"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            }
        };
    }
}