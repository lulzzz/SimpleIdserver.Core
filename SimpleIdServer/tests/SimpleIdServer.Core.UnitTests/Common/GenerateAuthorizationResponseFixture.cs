using Moq;
using Newtonsoft.Json;
using SimpleIdServer.Core;
using SimpleIdServer.Core.Api.Authorization;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.IdentityStore.Models;
using SimpleIdServer.IdentityStore.Repositories;
using SimpleIdServer.Lib;
using SimpleIdServer.OAuth.Logging;
using SimpleIdServer.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Common
{
    public sealed  class GenerateAuthorizationResponseFixture
    {
        private Mock<IAuthorizationCodeStore> _authorizationCodeRepositoryFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;
        private Mock<ITokenStore> _grantedTokenRepositoryFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IAuthorizationFlowHelper> _authorizationFlowHelperFake;                
        private Mock<IClientHelper> _clientHelperFake;
        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;
        private Mock<IUserRepository> _userRepoStub;
        private IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public static string ToHexString(IEnumerable<byte> arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            var sb = new StringBuilder();
            foreach (var s in arr)
            {
                sb.Append(s.ToString("x2"));
            }

            return sb.ToString();
        }

        [Fact]
        public void WhenGenerateSessionState()
        {
            // 0cb582e736597d717f0f6b34b987ea5ad0a6a82c1294fa37e2d75a444da782aa
            // 0cb582e736597d717f0f6b34b987ea5ad0a6a82c1294fa37e2d75a444da782aa
            const string clientId = "ResourceManagerClientId";
            const string originUrl = "http://localhost:64950";
            const string sessionId = "d95d6ea3-36f5-4ccd-886a-d469210f8e33";
            const string salt = "a781f21b-a9e0-4b84-90ed-2dc4535ac927";
            var bytes = Encoding.UTF8.GetBytes(clientId + originUrl + sessionId + salt);
            byte[] hash;
            using (var sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            var hashed = ToHexString(hash);
            var b = hashed.Base64Encode();
            string s = "";
        }

        [Fact]
        public async Task When_Passing_No_Action_Result_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAuthorizationResponse.ExecuteAsync(null, null, null, null, null));
        }

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var redirectInstruction = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            
            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAuthorizationResponse.ExecuteAsync(redirectInstruction, null, null, null, null));
        }

        [Fact]
        public async Task When_There_Is_No_Logged_User_Then_Exception_Is_Throw()
        {
            // ARRANGE
            InitializeFakeObjects();
            var redirectInstruction = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAuthorizationResponse.ExecuteAsync(redirectInstruction, new AuthorizationParameter(), null, null, null));
        }

        [Fact]
        public async Task When_No_Client_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var redirectInstruction = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAuthorizationResponse.ExecuteAsync(redirectInstruction, new AuthorizationParameter(), null, null, null));
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            var authorizationParameter = new AuthorizationParameter();
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _clientHelperFake.Setup(c => c.GenerateIdTokenAsync(It.IsAny<string>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(idToken));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERT
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Constants.StandardAuthorizationResponseNames.IdTokenName));
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == idToken));
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_No_Granted_Token_Then_Token_Is_Generated_And_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = Guid.NewGuid().ToString()
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(() => new List<string> { scope });
            _grantedTokenHelperStub.Setup(r => r.GetValidGrantedTokenAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult((GrantedToken)null));
            _grantedTokenGeneratorHelperFake.Setup(r => r.GenerateTokenAsync(It.IsAny<Client>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(grantedToken));
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Constants.StandardAuthorizationResponseNames.AccessTokenName));
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == grantedToken.AccessToken));
            _grantedTokenRepositoryFake.Verify(g => g.AddToken(grantedToken));
            _oauthEventSource.Verify(e => e.GrantAccessToClient(clientId, grantedToken.AccessToken, scope));
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_A_GrantedToken_Then_Token_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = Guid.NewGuid().ToString()
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(() => new List<string> { scope });
            _grantedTokenHelperStub.Setup(r => r.GetValidGrantedTokenAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(() => Task.FromResult(grantedToken));
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Constants.StandardAuthorizationResponseNames.AccessTokenName));
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == grantedToken.AccessToken));
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_AuthorizationCode_Then_Code_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var consent = new Consent();
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.code  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            _authorizationCodeRepositoryFake.Verify(a => a.AddAuthorizationCode(It.IsAny<AuthorizationCode>()));
            _oauthEventSource.Verify(s => s.GrantAuthorizationCodeToClient(clientId, It.IsAny<string>(), scope));
        }

        [Fact]
        public async Task When_An_Authorization_Response_Is_Generated_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType
            };
            var client = new Client
            {
                IdTokenEncryptedResponseAlg = SimpleIdServer.Core.Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = SimpleIdServer.Core.Jwt.Constants.JweEncNames.A128CBC_HS256,
                IdTokenSignedResponseAlg = SimpleIdServer.Core.Jwt.Constants.JwsAlgNames.RS256
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERT
            _oauthEventSource.Verify(s => s.StartGeneratingAuthorizationResponseToClient(clientId, responseType));
            _oauthEventSource.Verify(s => s.EndGeneratingAuthorizationResponseToClient(clientId, JsonConvert.SerializeObject(actionResult.RedirectInstruction.Parameters)));
        }

        [Fact]
        public async Task When_Redirecting_To_Callback_And_There_Is_No_Response_Mode_Specified_Then_The_Response_Mode_Is_Set()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType,
                ResponseMode = ResponseMode.None
            };
            var client = new Client
            {
                IdTokenEncryptedResponseAlg = SimpleIdServer.Core.Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = SimpleIdServer.Core.Jwt.Constants.JweEncNames.A128CBC_HS256,
                IdTokenSignedResponseAlg = SimpleIdServer.Core.Jwt.Constants.JwsAlgNames.RS256
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = TypeActionResult.RedirectToCallBackUrl
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopesAsync(It.IsAny<IList<Claim>>(), It.IsAny<AuthorizationParameter>(), null, null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<IList<Claim>>()))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(), It.IsAny<JweAlg>(), It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(idToken));
            _authorizationFlowHelperFake.Setup(
                a => a.GetAuthorizationFlow(It.IsAny<ICollection<ResponseType>>(), It.IsAny<string>()))
                .Returns(AuthorizationFlow.ImplicitFlow);
            _userRepoStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new User
            {
                Claims = new List<Claim>
                {
                    new Claim("sub", "fake")
                }
            }));

            // ACT
            await _generateAuthorizationResponse.ExecuteAsync(actionResult, authorizationParameter, new Client(), null, "fake");

            // ASSERT
            Assert.True(actionResult.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        private void InitializeFakeObjects()
        {
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeStore>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _grantedTokenRepositoryFake = new Mock<ITokenStore>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authorizationFlowHelperFake = new Mock<IAuthorizationFlowHelper>();
            _clientHelperFake = new Mock<IClientHelper>();
            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();
            _userRepoStub = new Mock<IUserRepository>();
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                _authorizationCodeRepositoryFake.Object,
                _grantedTokenRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _jwtGeneratorFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _consentHelperFake.Object,
                _oauthEventSource.Object,
                _authorizationFlowHelperFake.Object,
                _clientHelperFake.Object,
                _grantedTokenHelperStub.Object,
                _userRepoStub.Object);
        }
    }
}
