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

using Moq;
using SimpleIdentityServer.Core.UnitTests.Fake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.Jwt.Encrypt;
using SimpleIdServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdServer.Core.Jwt.Mapping;
using SimpleIdServer.Core.Jwt.Serializer;
using SimpleIdServer.Core.Jwt.Signature;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Validators;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    public class JwtGeneratorFixture
    {
        private IJwtGenerator _jwtGenerator;
        private Mock<IConfigurationService> _simpleIdentityServerConfigurator;
        private Mock<IClientRepository> _clientRepositoryStub;                
        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryStub;
        private Mock<IScopeRepository> _scopeRepositoryStub;

        #region GenerateAccessToken

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateAccessToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();

            // ACT & ASSERT
           await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateAccessToken(new Client(), null, null));
        }

        [Fact]
        public async Task When_Generate_AccessToken()
        {
            // ARRANGE
            const string clientId = "client_id";
            var scopes = new List<string> { "openid", "role" };
            InitializeMockObjects();
            var client = new Client
            {
                ClientId = clientId
            };
            _simpleIdentityServerConfigurator.Setup(g => g.GetTokenValidityPeriodInSecondsAsync()).Returns(Task.FromResult((double)3600));

            // ACT
            var result = await _jwtGenerator.GenerateAccessToken(client, scopes, null);

            // ASSERTS.
            Assert.NotNull(result);
        }


        #endregion

        #region GeneratedIdTokenPayloadForScopes

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateIdTokenPayloadForScopes_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(null, authorizationParameter, null));
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_IndicateTheMaxAge_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                MaxAge = 2
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT
            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claims, authorizationParameter, null, currentDateTimeOffset);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.ContainsKey(StandardClaimNames.AuthenticationTime));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Subject].ToString().Equals(subject));
            Assert.NotEmpty(result[StandardClaimNames.AuthenticationTime].ToString());
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_NumberOfAudiencesIsMoreThanOne_Then_Azp_Should_Be_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            var clientId = FakeOpenIdAssets.GetClients().First().ClientId;
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT
            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claims, authorizationParameter, issuerName);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.Audiences.Count() > 1);
            Assert.True(result.Azp == clientId);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_ThereNoClient_Then_Azp_Should_Be_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult((IEnumerable<Client>)new List<Client>()));

            // ACT
            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claims, authorizationParameter, issuerName);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.Audiences.Count() == 1);
            Assert.True(result.Azp == clientId);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_With_No_Authorization_Request_Then_MandatoriesClaims_Are_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            const string subject = "habarthierry@hotmail.fr";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT
            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claims, authorizationParameter, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.ContainsKey(StandardClaimNames.Audiences));
            Assert.True(result.ContainsKey(StandardClaimNames.ExpirationTime));
            Assert.True(result.ContainsKey(StandardClaimNames.Iat));
            Assert.True(result.GetClaimValue(Constants.StandardResourceOwnerClaimNames.Subject) == subject);
        }

        #endregion

        #region GenerateFilteredIdTokenPayload

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateFilteredIdTokenPayload_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(null, authorizationParameter, null, null));
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Audiences_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValuesName,
                            new [] { "audience" }
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                    claims,
                    authorizationParameter,
                    claimsParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Audiences));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Issuer_Claim_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Issuer,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            "issuer"
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                    claims,
                    authorizationParameter,
                    claimsParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Issuer));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_ExpirationTime_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.ExpirationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            12
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                    claims,
                    authorizationParameter,
                    claimsParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.ExpirationTime));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_PassingANotValidClaimValue_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string notValidSubject = "habarthierry@hotmail.be";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter();
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            notValidSubject
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT & ASSERTS
            var result = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                claims,
                authorizationParameter,
                claimsParameter, null));

            Assert.True(result.Code.Equals(ErrorCodes.InvalidGrant));
            Assert.True(result.Message.Equals(string.Format(ErrorDescriptions.TheClaimIsNotValid, Constants.StandardResourceOwnerClaimNames.Subject)));
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string nonce = "nonce";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject),
                new Claim(Constants.StandardResourceOwnerClaimNames.Role, "['role1', 'role2']")
            };
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = nonce
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.AuthenticationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValuesName,
                            new [] { FakeOpenIdAssets.GetClients().First().ClientId }
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = StandardClaimNames.Nonce,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            nonce
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Role,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT
            var result = await _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                claims,
                authorizationParameter,
                claimsParameter, null, currentDateTimeOffset);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Role));
            Assert.True(result.ContainsKey(StandardClaimNames.AuthenticationTime));
            Assert.True(result.ContainsKey(StandardClaimNames.Nonce));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Subject].ToString().Equals(subject));
            Assert.True(long.Parse(result[StandardClaimNames.AuthenticationTime].ToString()).Equals(currentDateTimeOffset));
        }

        #endregion

        #region GenerateUserInfoPayloadForScope

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(new AuthorizationParameter(), null));
        }

        [Fact]
        public async Task When_Requesting_UserInformation_JwsPayload_For_Scopes_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT
            var result = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(authorizationParameter, claims);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Name));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Subject].ToString().Equals(subject));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Name].ToString().Equals(name));
        }
        
        #endregion

        #region GenerateFilteredUserInfoPayload

        [Fact]
        public void When_Passing_Null_Parameters_To_GenerateFilteredUserInfoPayload_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();
            
            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(null, authorizationParameter, null));
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Subject_Is_Empty_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(claimsParameter, authorizationParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Subject_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "invalid@loki.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            "habarthierry@lokie.be"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                authorizationParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(exception.State == state);
        }
        
        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Name_Is_Empty_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                authorizationParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Constants.StandardResourceOwnerClaimNames.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Name_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@lokie.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject),
                new Claim(Constants.StandardResourceOwnerClaimNames.Name, "invalid_name")
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            subject
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            "name"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                authorizationParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Constants.StandardResourceOwnerClaimNames.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_For_Some_Valid_Claims_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            SimpleIdServer.Core.Constants.StandardClaimParameterValueNames.ValueName,
                            subject
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            // ACT
            var result = _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                authorizationParameter, claims);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.ContainsKey(Constants.StandardResourceOwnerClaimNames.Name));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Subject].ToString().Equals(subject));
            Assert.True(result[Constants.StandardResourceOwnerClaimNames.Name].ToString().Equals(name));
        }

        #endregion

        #region FillInOtherClaimsIdentityTokenPayload

        [Fact]
        public void When_Passing_Null_Parameters_To_FillInOtherClaimsIdentityTokenPayload_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(new JwsPayload(), null, null, null, null));
        }

        [Fact]
        public void When_JwsAlg_Is_None_And_Trying_To_FillIn_Other_Claims_Then_The_Properties_Are_Not_Filled_In()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = "none";
            var jwsPayload = new JwsPayload();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = client.ClientId
            };

            // ACT & ASSERT
            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, null, null, authorizationParameter, new Client());
            Assert.False(jwsPayload.ContainsKey(StandardClaimNames.AtHash));
            Assert.False(jwsPayload.ContainsKey(StandardClaimNames.CHash));
        }

        [Fact]
        public void When_JwsAlg_Is_RS256_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = Constants.JwsAlgNames.RS256;
            var jwsPayload = new JwsPayload();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = client.ClientId
            };

            // ACT & ASSERT
            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, "authorization_code", "access_token", authorizationParameter, client);
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.CHash));
        }

        [Fact]
        public void When_JwsAlg_Is_RS384_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = Constants.JwsAlgNames.RS384;
            var jwsPayload = new JwsPayload();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = client.ClientId
            };

            // ACT & ASSERT
            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, "authorization_code", "access_token", authorizationParameter, client);
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.CHash));
        }
        
        [Fact]
        public void When_JwsAlg_Is_RS512_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = Constants.JwsAlgNames.RS512;
            var jwsPayload = new JwsPayload();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = client.ClientId
            };

            // ACT & ASSERT
            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, "authorization_code", "access_token", authorizationParameter, client);
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.ContainsKey(StandardClaimNames.CHash));
        }

        #endregion

        #region Encrypt 

        [Fact]
        public async Task When_Encrypt_Jws_Then_Jwe_Is_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenEncryptedResponseAlg = Constants.JweAlgNames.RSA1_5;
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            };

            var jsonWebKey = new JsonWebKey
            {
                Alg = AllAlg.RSA1_5,
                KeyOps = new[]
                    {
                       KeyOperations.Encrypt,
                       KeyOperations.Decrypt
                    },
                Kid = "3",
                Kty = KeyType.RSA,
                Use = Use.Enc,
                SerializedKey = serializedRsa,
            };
            var jws = "jws";
            ICollection<JsonWebKey> jwks = new List<JsonWebKey> { jsonWebKey };
            _jsonWebKeyRepositoryStub.Setup(j => j.GetByAlgorithmAsync(It.IsAny<Use>(), It.IsAny<AllAlg>(), It.IsAny<KeyOperations[]>()))
                .Returns(Task.FromResult(jwks));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(client));

            // ACT
            var jwe = await _jwtGenerator.EncryptAsync(jws,
                JweAlg.RSA1_5,
                JweEnc.A128CBC_HS256);

            // ASSERT
            Assert.NotEmpty(jwe);
        }

        #endregion

        #region Sign

        [Fact]
        public async Task When_Sign_Payload_Then_Jws_Is_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenEncryptedResponseAlg = Constants.JwsAlgNames.RS256;
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            };
            var jsonWebKey = new JsonWebKey
            {
                Alg = AllAlg.RS256,
                KeyOps = new[]
                {
                   KeyOperations.Sign,
                   KeyOperations.Verify
                },
                Kid = "a3rMUgMFv9tPclLa6yF3zAkfquE",
                Kty = KeyType.RSA,
                Use = Use.Sig,
                SerializedKey = serializedRsa
            };
            ICollection<JsonWebKey> jwks = new List<JsonWebKey> { jsonWebKey };
            _jsonWebKeyRepositoryStub.Setup(j => j.GetByAlgorithmAsync(It.IsAny<Use>(), It.IsAny<AllAlg>(), It.IsAny<KeyOperations[]>()))
                .Returns(Task.FromResult(jwks));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(client));
            var jwsPayload = new JwsPayload();

            // ACT
            var jws = await _jwtGenerator.SignAsync(jwsPayload,
                JwsAlg.RS256);

            // ASSERT
            Assert.NotEmpty(jws);
        }

        #endregion

        private void InitializeMockObjects()
        {
            _simpleIdentityServerConfigurator = new Mock<IConfigurationService>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _jsonWebKeyRepositoryStub = new Mock<IJsonWebKeyRepository>();
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            var clientValidator = new ClientValidator();
            var claimsMapping = new ClaimsMapping();
            var parameterParserHelper = new ParameterParserHelper(_scopeRepositoryStub.Object);
            var createJwsSignature = new CreateJwsSignature(new CngKeySerializer());
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);

            _jwtGenerator = new JwtGenerator(
                _simpleIdentityServerConfigurator.Object,
                _clientRepositoryStub.Object,
                clientValidator,
                _jsonWebKeyRepositoryStub.Object,
                _scopeRepositoryStub.Object,
                claimsMapping,
                parameterParserHelper,
                jwsGenerator,
                jweGenerator);
        }
    }
}