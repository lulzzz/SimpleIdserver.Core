﻿#region copyright
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
using System.Security.Cryptography;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Jwt;

namespace SimpleIdentityServer.Core.UnitTests.Fake
{
    public static class FakeOpenIdAssets
    {
        /// <summary>
        /// Get a list of fake clients
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "MyBlog",
                    ClientName = "My blog",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "MyBlog"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                    AllowedScopes = new List<Scope>
                    {
                        // PROTECTED API SCOPES
                        new Scope
                        {
                            Name = "BlogApi"
                        },
                        new Scope
                        {
                            Name = "BlogApi:AddArticle"
                        },
                        new Scope
                        {
                            Name = "openid",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            Description = "openid",
                            Type = ScopeType.ProtectedApi
                        },
                        // RO SCOPES
                        new Scope
                        {
                            Name = "profile",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            Description = "Access to the profile",
                            Claims = new List<string>
                            {
                                Constants.StandardResourceOwnerClaimNames.Name,
                                Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Constants.StandardResourceOwnerClaimNames.GivenName,
                                Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Constants.StandardResourceOwnerClaimNames.NickName,
                                Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Constants.StandardResourceOwnerClaimNames.Profile,
                                Constants.StandardResourceOwnerClaimNames.Picture,
                                Constants.StandardResourceOwnerClaimNames.WebSite,
                                Constants.StandardResourceOwnerClaimNames.Gender,
                                Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Constants.StandardResourceOwnerClaimNames.Locale,
                                Constants.StandardResourceOwnerClaimNames.UpdatedAt
                            },
                            Type = ScopeType.ResourceOwner
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
                                Constants.StandardResourceOwnerClaimNames.Email,
                                Constants.StandardResourceOwnerClaimNames.EmailVerified
                            },
                            Type = ScopeType.ResourceOwner
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
                                Constants.StandardResourceOwnerClaimNames.Address
                            },
                            Type = ScopeType.ResourceOwner
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
                                Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                                Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                            },
                            Type = ScopeType.ResourceOwner
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.@implicit,
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token,
                        ResponseType.code,
                        ResponseType.id_token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    // IdTokenEncryptedResponseAlg = "RSA1_5",
                    // IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new List<string>
                    {
                        "https://op.certification.openid.net:60360/authz_cb",
                        "http://localhost",
                        "https://op.certification.openid.net:60186/authz_cb"
                    }
                }
            };
        }

        /// <summary>
        /// Get a list of scopes
        /// </summary>
        /// <returns></returns>
        public static List<Scope> GetScopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "BlogApi",
                    Description = "Access to the blog API",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = false,
                    Description = "openid",
                    Type = ScopeType.ProtectedApi
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    Description = "Access to the profile",
                    Claims = new List<string>
                    {
                        Constants.StandardResourceOwnerClaimNames.Name,
                        Constants.StandardResourceOwnerClaimNames.FamilyName,
                        Constants.StandardResourceOwnerClaimNames.GivenName,
                        Constants.StandardResourceOwnerClaimNames.MiddleName,
                        Constants.StandardResourceOwnerClaimNames.NickName,
                        Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                        Constants.StandardResourceOwnerClaimNames.Profile,
                        Constants.StandardResourceOwnerClaimNames.Picture,
                        Constants.StandardResourceOwnerClaimNames.WebSite,
                        Constants.StandardResourceOwnerClaimNames.Gender,
                        Constants.StandardResourceOwnerClaimNames.BirthDate,
                        Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                        Constants.StandardResourceOwnerClaimNames.Locale,
                        Constants.StandardResourceOwnerClaimNames.UpdatedAt
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true
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
                        Constants.StandardResourceOwnerClaimNames.Email,
                        Constants.StandardResourceOwnerClaimNames.EmailVerified
                    },
                    Type = ScopeType.ResourceOwner
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
                        Constants.StandardResourceOwnerClaimNames.Address
                    },
                    Type = ScopeType.ResourceOwner
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
                        Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                        Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                    },
                    Type = ScopeType.ResourceOwner
                }
            };
        }
        
        public static List<Consent> GetConsents()
        {
            return new List<Consent>();
        }

        public static List<JsonWebKey> GetJsonWebKeys()
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            return new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new []
                    {
                        KeyOperations.Sign,
                        KeyOperations.Verify
                    },
                    Kid = "a3rMUgMFv9tPclLa6yF3zAkfquE",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa,
                },
                new JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = new []
                    {
                        KeyOperations.Encrypt,
                        KeyOperations.Decrypt
                    },
                    Kid = "3",
                    Kty = KeyType.RSA,
                    Use = Use.Enc,
                    SerializedKey = serializedRsa,
                }
            };
        }
    }
}
