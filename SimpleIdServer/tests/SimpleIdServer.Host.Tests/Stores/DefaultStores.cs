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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using Newtonsoft.Json;
using SimpleIdServer.Authenticate.LoginPassword;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Helpers;

namespace SimpleIdServer.Host.Tests.Stores
{
    public static class DefaultStores
    {
        public static List<Consent> Consents()
        {
            return new List<Consent>()
            {
                new Consent
                {
                    Id = "1",
                    Client = new Core.Common.Models.Client
                    {
                        ClientId = "authcode_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "2",
                    Client = new Core.Common.Models.Client
                    {
                        ClientId = "implicit_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "3",
                    Client = new Core.Common.Models.Client
                    {
                        ClientId = "hybrid_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "4",
                    Client = new Core.Common.Models.Client
                    {
                        ClientId = "pkce_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                }
            };
        }

        public static List<JsonWebKey> JsonWebKeys(SharedContext sharedContext)
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            return new List<JsonWebKey>
            {
                sharedContext.SignatureKey,
                sharedContext.EncryptionKey
            };
        }

        public static List<Scope> Scopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "access to the openid scope",
                    Type = ScopeType.ProtectedApi,
                    Claims = new List<string> { },
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    Description = "Access to the profile",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "scim",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    Description = "Access to the scim",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "email",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                    },
                    Type = ScopeType.ResourceOwner,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address
                    },
                    Type = ScopeType.ResourceOwner,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                    },
                    Type = ScopeType.ResourceOwner,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "role",
                    IsExposed = true,
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Description = "Access to your roles",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role
                    },
                    Type = ScopeType.ResourceOwner,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "register_client",
                    IsExposed = false,
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Description = "Register a client",
                    Type = ScopeType.ProtectedApi,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "manage_profile",
                    IsExposed = false,
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Description = "Manage the user's profiles",
                    Type = ScopeType.ProtectedApi,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                },
                new Scope
                {
                    Name = "manage_account_filtering",
                    IsExposed = false,
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Description = "Manage the account filtering",
                    Type = ScopeType.ProtectedApi,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                }
            };
        }

        public static List<ResourceOwner> Users()
        {
            return new List<ResourceOwner>
            {
                    new ResourceOwner
                    {
                        Id = "administrator",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator"),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "administrator"),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, "phone"),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address, "{ country : 'france' }")
                        },
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Value = PasswordHelper.ComputeHash("password"),
                                Type = "pwd"
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    },
                    new ResourceOwner
                    {
                        Id = "user",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "user")
                        },
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Value = PasswordHelper.ComputeHash("password"),
                                Type = "pwd"
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    },
                    new ResourceOwner
                    {
                        Id = "superuser",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "superuser"),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "[ 'administrator', 'role' ]")
                        },
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Value = PasswordHelper.ComputeHash("password"),
                                Type = "pwd"
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    },
                    new ResourceOwner
                    {
                        Id = "blockeduser",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "blockeduser")
                        },
                        IsBlocked = true,
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                Value = PasswordHelper.ComputeHash("password"),
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "pwd",
                                IsBlocked = true
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    },
                    new ResourceOwner
                    {
                        Id = "toomanyattemps",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "toomanyattemps")
                        },
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                NumberOfAttempts = 10,
                                FirstAuthenticationFailureDateTime = DateTime.UtcNow.AddSeconds(-1),
                                Value = PasswordHelper.ComputeHash("password"),
                                Type = "pwd"
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    },
                    new ResourceOwner
                    {
                        Id = "expired",
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "expired")
                        },
                        Credentials = new List<ResourceOwnerCredential>
                        {
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(-2),
                                Value = PasswordHelper.ComputeHash("password"),
                                Type = "pwd"
                            },
                            new ResourceOwnerCredential
                            {
                                ExpirationDateTime = DateTime.UtcNow.AddDays(2),
                                Type = "sms"
                            }
                        }
                    }
            };
        }

        public static List<CredentialSetting> GetCredentialSettings()
        {
            return new List<CredentialSetting>
            {
                new CredentialSetting
                {
                    CredentialType = "pwd",
                    AuthenticationIntervalsInSeconds = 2000000000,
                    IsBlockAccountPolicyEnabled  = true,
                    Options = JsonConvert.SerializeObject(new PwdCredentialOptions
                    {
                        IsRegexEnabled = true,
                        PasswordDescription = "at least 8 characters, 2 letters, 2 digits, 1 upper case, 1 lower case and 1 symbol",
                        RegularExpression = @"^(?=(.*\d){2})(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\d]).{8,}$"
                    }),
                    NumberOfAuthenticationAttempts = 2
                },
                new CredentialSetting
                {
                    CredentialType = "sms",
                    AuthenticationIntervalsInSeconds = 2000000000,
                   NumberOfAuthenticationAttempts = 2,
                   IsBlockAccountPolicyEnabled = true
                }
            };
        }

        public static List<Core.Common.Models.Client> Clients(SharedContext sharedCtx)
        {
            return new List<Core.Common.Models.Client>
            {
                new Core.Common.Models.Client
                {
                    ClientId = "client",
                    ClientName = "client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        },
                        new Scope
                        {
                            Name = "address"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.refresh_token,
                        GrantType.password
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string>
                    {
                        "https://localhost:4200/callback"
                    }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "client_userinfo_sig_rs256",
                    ClientName = "client_userinfo_sig_rs256",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client_userinfo_sig_rs256"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.refresh_token,
                        GrantType.password
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    UserInfoSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "client_userinfo_enc_rsa15",
                    ClientName = "client_userinfo_enc_rsa15",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client_userinfo_enc_rsa15"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.refresh_token,
                        GrantType.password
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    UserInfoSignedResponseAlg = "RS256",
                    UserInfoEncryptedResponseAlg = "RSA1_5",
                    UserInfoEncryptedResponseEnc = "A128CBC-HS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "clientWithWrongResponseType",
                    ClientName = "clientWithWrongResponseType",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "clientWithWrongResponseType"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.refresh_token,
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "clientCredentials",
                    ClientName = "clientCredentials",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "clientCredentials"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.refresh_token,
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "basic_client",
                    ClientName = "basic_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "basic_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "post_client",
                    ClientName = "post_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "post_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "jwt_client",
                    ClientName = "jwt_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "jwt_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_jwt,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" },
                    JsonWebKeys = new List<JsonWebKey>
                    {
                        sharedCtx.ModelSignatureKey,
                        sharedCtx.ModelEncryptionKey
                    }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "private_key_client",
                    ClientName = "private_key_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "private_key_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.private_key_jwt,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "https://localhost:4200/callback" },
                    JwksUri = "http://localhost:5000/jwks_client"
                },
                new Core.Common.Models.Client
                {
                    ClientId = "authcode_client",
                    ClientName = "authcode_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "authcode_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "http://localhost:5000/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "incomplete_authcode_client",
                    ClientName = "incomplete_authcode_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "incomplete_authcode_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "http://localhost:5000/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "implicit_client",
                    ClientName = "implicit_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "implicit_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.@implicit
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "http://localhost:5000/callback" }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "pkce_client",
                    ClientName = "pkce_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "pkce_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "http://localhost:5000/callback" },
                    RequirePkce = true
                },
                new Core.Common.Models.Client
                {
                    ClientId = "hybrid_client",
                    ClientName = "hybrid_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "hybrid_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code,
                        GrantType.@implicit
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string> { "http://localhost:5000/callback" },
                },
                // Certificate test client.
                new Core.Common.Models.Client
                {
                    ClientId = "certificate_client",
                    ClientName = "Certificate test client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.X509Thumbprint,
                            Value = "E831DB1512E5AE431B6CDC6355CDA4CBBDB9CAAC"
                        },
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.X509Name,
                            Value = "CN=localhost"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.tls_client_auth,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.password
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.native
                },
                // Client credentials + stateless access token.
                new Core.Common.Models.Client
                {
                    ClientId = "stateless_client",
                    ClientName = "Stateless client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "stateless_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "register_client"
                        },
                        new Scope
                        {
                            Name = "manage_profile"
                        },
                        new Scope
                        {
                            Name = "manage_account_filtering"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.native
                }
            };
        }        
    }
}