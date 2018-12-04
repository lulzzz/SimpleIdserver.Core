using Newtonsoft.Json.Linq;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Uma.Core.JwtToken;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Policies
{
    public interface IBasicAuthorizationPolicy
    {
        Task<ResourceValidationResult> Execute(string openidProvider, ResourceSet resource, TicketLineParameter ticket, ClaimTokenParameter claimTokenParameters);
    }

    public class ResourceValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<AuthorizationPolicyResult> AuthorizationPoliciesResult { get; set; }
    }

    internal class BasicAuthorizationPolicy : IBasicAuthorizationPolicy
    {
        private readonly IJwtTokenParser _jwtTokenParser;
        private readonly IPendingRequestRepository _pendingRequestRepository;
        
        public BasicAuthorizationPolicy(IJwtTokenParser jwtTokenParser, IPendingRequestRepository pendingRequestRepository)
        {
            _jwtTokenParser = jwtTokenParser;
            _pendingRequestRepository = pendingRequestRepository;
        }

        #region Public methods

        public async Task<ResourceValidationResult> Execute(string openidProvider, ResourceSet resource, TicketLineParameter ticketLineParameter, ClaimTokenParameter claimTokenParameter)
        {
            if (string.IsNullOrWhiteSpace(openidProvider))
            {
                throw new ArgumentNullException(nameof(openidProvider));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (ticketLineParameter == null)
            {
                throw new ArgumentNullException(nameof(ticketLineParameter));
            }

            JwsPayload jwsPayload = null;
            string subject = null;
            if (claimTokenParameter != null && claimTokenParameter.Format == Constants.IdTokenType)
            {
                var idToken = claimTokenParameter.Token;
                jwsPayload = await _jwtTokenParser.UnSign(idToken, openidProvider).ConfigureAwait(false);
                var subjectClaim = jwsPayload.FirstOrDefault(c => c.Key == SimpleIdServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                if (!subjectClaim.Equals(default(KeyValuePair<string, object>)) && subjectClaim.Value != null)
                {
                    subject = subjectClaim.Value.ToString();
                }
            }

            var validationsResult = new List<AuthorizationPolicyResult>();
            var policies = resource.AuthPolicies;
            if (policies != null && policies.Any())
            {
                foreach (var policy in policies)
                {
                    if (ticketLineParameter.Scopes.Any(s => !policy.Scopes.Contains(s)))
                    {
                        validationsResult.Add(new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.NotAuthorized,
                            Policy = policy,
                            ErrorDetails = "the scope is not valid"
                        });
                        continue;
                    }

                    var clientAuthorizationResult = CheckClients(policy, ticketLineParameter);
                    if (clientAuthorizationResult != null && clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
                    {
                        validationsResult.Add(clientAuthorizationResult);
                        continue;
                    }

                    var validationResult = CheckClaims(openidProvider, policy, jwsPayload);
                    validationsResult.Add(validationResult);
                }
            }


            var vr = validationsResult.FirstOrDefault(v => v.Type == AuthorizationPolicyResultEnum.Authorized);
            if (vr != null)
            {
                return new ResourceValidationResult
                {
                    IsValid = true
                };
            }

            if (!resource.AcceptPendingRequest)
            {
                return new ResourceValidationResult
                {
                    IsValid = false,
                    AuthorizationPoliciesResult = validationsResult
                };
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                var pendingRequestLst = await _pendingRequestRepository.Get(resource.Id, subject).ConfigureAwait(false);
                var pendingRequest = pendingRequestLst.FirstOrDefault(p => ticketLineParameter.Scopes.Count() == p.Scopes.Count() && ticketLineParameter.Scopes.All(s => p.Scopes.Contains(s)));
                if (pendingRequest == null)
                {
                    await _pendingRequestRepository.Add(new PendingRequest
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreateDateTime = DateTime.UtcNow,
                        ResourceId = resource.Id,
                        IsConfirmed = false,
                        RequesterSubject = subject,
                        Scopes = ticketLineParameter.Scopes
                    }).ConfigureAwait(false);
                    return new ResourceValidationResult
                    {
                        IsValid = false,
                        AuthorizationPoliciesResult = new[]
                        {
                        new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.RequestSubmitted,
                            ErrorDetails = "a request has been submitted"
                        }
                    }
                    };
                }
            }

            return new ResourceValidationResult
            {
                IsValid = false,
                AuthorizationPoliciesResult = new[]
                {
                    new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.RequestNotConfirmed,
                        ErrorDetails = "the request is not confirmed by the resource owner"
                    }
                }
            };
        }

        #endregion

        #region Private methods

        private AuthorizationPolicyResult GetNeedInfoResult(List<Claim> claims, string openidConfigurationUrl)
        {
            var requestingPartyClaims = new Dictionary<string, object>();
            var requiredClaims = new List<Dictionary<string, string>>();
            foreach (var claim in claims)
            {
                requiredClaims.Add(new Dictionary<string, string>
                {
                    {
                        Constants.ErrorDetailNames.ClaimName, claim.Type
                    },
                    {
                        Constants.ErrorDetailNames.ClaimFriendlyName, claim.Type
                    },
                    {
                        Constants.ErrorDetailNames.ClaimIssuer, openidConfigurationUrl
                    }
                });
            }

            requestingPartyClaims.Add(Constants.ErrorDetailNames.RequiredClaims, requiredClaims);
            requestingPartyClaims.Add(Constants.ErrorDetailNames.RedirectUser, false);
            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.NeedInfo,
                ErrorDetails = new Dictionary<string, object>
                {
                    {
                        Constants.ErrorDetailNames.RequestingPartyClaims,
                        requestingPartyClaims
                    }
                }
            };
        }

        private AuthorizationPolicyResult CheckClaims(string openidProvider, Policy authorizationPolicy, JwsPayload jwsPayload)
        {
            if (authorizationPolicy.Claims == null || !authorizationPolicy.Claims.Any())
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized,
                    Policy = authorizationPolicy
                };
            }

            if (jwsPayload == null)
            {
                var tmp = GetNeedInfoResult(authorizationPolicy.Claims, openidProvider);
                tmp.Policy = authorizationPolicy;
                return tmp;
            }

            foreach (var claim in authorizationPolicy.Claims)
            {
                var payload = jwsPayload.FirstOrDefault(j => j.Key == claim.Type);
                if (payload.Equals(default(KeyValuePair<string, object>)))
                {
                    return new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.NotAuthorized,
                        Policy = authorizationPolicy,
                        ErrorDetails = "the user is not authorized"
                    };
                }

                IEnumerable<string> claims = null;
                if (payload.Value is string)
                {
                    claims = new[] { payload.Value.ToString() };
                }
                else
                {
                    var arr = payload.Value as object[];
                    var jArr = payload.Value as JArray;
                    if (arr != null)
                    {
                        claims = arr.Select(c => c.ToString());
                    }

                    if (jArr != null)
                    {
                        claims = jArr.Select(c => c.ToString());
                    }
                }

                if (claims == null || !claims.Any(v => claim.Value == v))
                {
                    return new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.NotAuthorized,
                        Policy = authorizationPolicy,
                        ErrorDetails = "the user is not authorized"
                    };
                }
            }

            var subjectClaim = jwsPayload.FirstOrDefault(c => c.Key == SimpleIdServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var subject = string.Empty;
            if (!subjectClaim.Equals(default(KeyValuePair<string, object>)) && subjectClaim.Value != null)
            {
                subject = subjectClaim.Value.ToString();
            }

            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.Authorized,
                Policy = authorizationPolicy,
                Subject = subject
            };
        }

        private AuthorizationPolicyResult CheckClients(Policy authorizationPolicy, TicketLineParameter ticketLineParameter)
        {
            if (authorizationPolicy.ClientIds == null || !authorizationPolicy.ClientIds.Any())
            {
                return null;
            }

            if (!authorizationPolicy.ClientIds.Contains(ticketLineParameter.ClientId))
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized,
                    Policy = authorizationPolicy,
                    ErrorDetails = "the client is not authorized"
                };
            }

            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.Authorized
            };
        }

        #endregion
    }
}
