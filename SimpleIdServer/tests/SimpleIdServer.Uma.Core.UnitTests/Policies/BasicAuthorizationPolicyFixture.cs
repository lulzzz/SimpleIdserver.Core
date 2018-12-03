using Moq;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Uma.Core.JwtToken;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Policies;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdServer.Uma.Core.UnitTests.Policies
{
    public class BasicAuthorizationPolicyFixture
    {
        private Mock<IJwtTokenParser> _jwtTokenParserStub;
        private Mock<IPendingRequestRepository> _pendingRequestRepositorySub;
        private IBasicAuthorizationPolicy _basicAuthorizationPolicy;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute("openid", null, null, null));
        }
        
        [Fact]
        public async Task When_Doesnt_have_Permission_To_Access_To_Scope_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.True(result.IsValid == false);
        }

        [Fact]
        public async Task When_Client_Is_Not_Allowed_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    ClientIds = new List<string>
                    {
                        "client_id"
                    },
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    }
                }
            };
            
            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_There_Is_No_Access_Token_Passed_Then_NeedInfo_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "name"
                        },
                        new Claim
                        {
                            Type = "email"
                        }
                    }
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "bad_format",
                Token = "token"
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameter);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
            var errorDetails = error.ErrorDetails as Dictionary<string, object>;
            Assert.NotNull(errorDetails);
            Assert.True(errorDetails.ContainsKey(Constants.ErrorDetailNames.RequestingPartyClaims));
            var requestingPartyClaims = errorDetails[Constants.ErrorDetailNames.RequestingPartyClaims] as Dictionary<string, object>;
            Assert.NotNull(requestingPartyClaims);
            Assert.True(requestingPartyClaims.ContainsKey(Constants.ErrorDetailNames.RequiredClaims));
            Assert.True(requestingPartyClaims.ContainsKey(Constants.ErrorDetailNames.RedirectUser));
            var requiredClaims = requestingPartyClaims[Constants.ErrorDetailNames.RequiredClaims] as List<Dictionary<string, string>>;
            Assert.NotNull(requiredClaims);
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimName && kv.Value == "name")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "name")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimName && kv.Value == "email")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "email")));
        }

        [Fact]
        public async Task When_JwsPayload_Cannot_Be_Extracted_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "name"
                        },
                        new Claim
                        {
                            Type = "email"
                        }
                    }
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult((JwsPayload)null));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }

        [Fact]
        public async Task When_Role_Is_Not_Correct_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "role",
                            Value = "role1"
                        },
                        new Claim
                        {
                            Type = "role",
                            Value = "role2"
                        }
                    }
                }
            };

            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult(new JwsPayload
                {
                    {
                        "role", "role1,role3"
                    }
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameter);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }

        [Fact]
        public async Task When_There_Is_No_Role_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "role",
                            Value = "role1"
                        },
                        new Claim
                        {
                            Type = "role",
                            Value = "role2"
                        }
                    }
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult(new JwsPayload()));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Roles_In_JArray_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "role",
                            Value = "role1"
                        },
                        new Claim
                        {
                            Type = "role",
                            Value = "role2"
                        }
                    }
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            var payload = new JwsPayload();
            payload.Add("role", new JArray("role3"));
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult(payload));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }
    
        [Fact]
        public async Task When_Passing_Not_Valid_Roles_InStringArray_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "role",
                            Value = "role1"
                        },
                        new Claim
                        {
                            Type = "role",
                            Value = "role2"
                        }
                    }
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            var payload = new JwsPayload();
            payload.Add("role", new string[] { "role3" });
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult(payload));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, claimTokenParameter);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }

        [Fact]
        public async Task When_Claims_Are_Not_Corred_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new List<Policy>
            {
                new Policy
                {
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    },
                    Claims = new List<Claim>
                    {
                        new Claim
                        {
                            Type = "name",
                            Value = "name"
                        },
                        new Claim
                        {
                            Type = "email",
                            Value = "email"
                        }
                    }
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Policy>()))
                .Returns(Task.FromResult(new JwsPayload
                {
                    {
                        "name", "bad_name"
                    }
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicy, claimTokenParameter);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.NeedInfo);
        }
        
        [Fact]
        public async Task When_ResourceOwnerConsent_Is_Required_Then_RequestSubmitted_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new List<string>
                    {
                        "read",
                        "create",
                        "update"
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.True(error.Type == AuthorizationPolicyResultEnum.RequestSubmitted);
        }

        [Fact]
        public async Task When_AuthPolicy_Needs_ResourceConsent_Then_Request_Is_Submitted()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "create"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new List<string>
                    {
                        "create"
                    }
                }
            };

            _pendingRequestRepositorySub.Setup(v => v.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((PendingRequest)null));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.Equal(AuthorizationPolicyResultEnum.RequestSubmitted, error.Type);
        }

        [Fact]
        public async Task When_Request_NotConfirmed_By_ResourceOwner_Then_NotAuthorized()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "create"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new List<string>
                    {
                        "create"
                    }
                }
            };

            _pendingRequestRepositorySub.Setup(v => v.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new PendingRequest
                {
                    IsConfirmed = false
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.False(result.IsValid);
            var error = result.AuthorizationPoliciesResult.First();
            Assert.Equal(AuthorizationPolicyResultEnum.RequestNotConfirmed, error.Type);
        }

        [Fact]
        public async Task When_Request_Confirmed_By_ResourceOwner_Then_Authorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "create"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new List<string>
                    {
                        "create"
                    }
                }
            };

            _pendingRequestRepositorySub.Setup(v => v.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new PendingRequest
                {
                    IsConfirmed = true
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task When_AuthorizationPassed_Then_Authorization_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new TicketLineParameter
            {
                Scopes = new List<string>
                {
                    "create"
                }
            };

            var authorizationPolicies = new List<Policy>
            {
                new Policy
                {
                    IsResourceOwnerConsentNeeded = false,
                    Scopes = new List<string>
                    {
                        "create"
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute("openid", ticket, authorizationPolicies, null);

            // ASSERT
            Assert.True(result.IsValid);
        }

        private void InitializeFakeObjects()
        {
            _jwtTokenParserStub = new Mock<IJwtTokenParser>();
            _pendingRequestRepositorySub = new Mock<IPendingRequestRepository>();
            _basicAuthorizationPolicy = new BasicAuthorizationPolicy(_jwtTokenParserStub.Object, _pendingRequestRepositorySub.Object);
        }
    }
}
