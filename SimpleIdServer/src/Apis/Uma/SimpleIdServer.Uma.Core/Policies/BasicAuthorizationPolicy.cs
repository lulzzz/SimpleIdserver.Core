using Newtonsoft.Json.Linq;
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
        Task<ResourceValidationResult> Execute(string openidProvider, TicketLineParameter ticket, IEnumerable<Policy> policies, ClaimTokenParameter claimTokenParameters);
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

        public async Task<ResourceValidationResult> Execute(string openidProvider, TicketLineParameter ticketLineParameter, IEnumerable<Policy> policies, ClaimTokenParameter claimTokenParameter)
        {
            if (string.IsNullOrWhiteSpace(openidProvider))
            {
                throw new ArgumentNullException(nameof(openidProvider));
            }

            if (ticketLineParameter == null)
            {
                throw new ArgumentNullException(nameof(ticketLineParameter));
            }

            if (policies == null || !policies.Any())
            {
                return new ResourceValidationResult
                {
                    IsValid = true
                };
            }

            var validationsResult = new List<AuthorizationPolicyResult>();
            foreach (var policy in policies)
            {
                if (ticketLineParameter.Scopes.Any(s => !policy.Scopes.Contains(s)))
                {
                    validationsResult.Add(new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.NotAuthorized
                    });
                    continue;
                }

                var clientAuthorizationResult = CheckClients(policy, ticketLineParameter);
                if (clientAuthorizationResult != null && clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
                {
                    validationsResult.Add(clientAuthorizationResult);
                    continue;
                }

                var validationResult = await CheckClaims(openidProvider, policy, claimTokenParameter).ConfigureAwait(false);
                validationsResult.Add(validationResult);
            }

            var vr = validationsResult.FirstOrDefault(v => v.Type == AuthorizationPolicyResultEnum.Authorized);
            if (vr == null)
            {
                return new ResourceValidationResult
                {
                    IsValid = false,
                    AuthorizationPoliciesResult = validationsResult
                };
            }

            if (vr.Policy.IsResourceOwnerConsentNeeded)
            {
                var pendingRequest = await _pendingRequestRepository.Get(vr.Policy.Id, vr.Subject).ConfigureAwait(false);
                if (pendingRequest == null)
                {
                    await _pendingRequestRepository.Add(new PendingRequest
                    {
                        CreateDateTime = DateTime.UtcNow,
                        AuthorizationPolicyRuleId = vr.Policy.Id,
                        IsConfirmed = false,
                        RequesterSubject = vr.Subject
                    }).ConfigureAwait(false);
                    return new ResourceValidationResult
                    {
                        IsValid = false,
                        AuthorizationPoliciesResult = new[]
                        {
                            new AuthorizationPolicyResult
                            {
                                Type = AuthorizationPolicyResultEnum.RequestSubmitted,
                                Policy = vr.Policy
                            }
                        }
                    };
                }

                if (!pendingRequest.IsConfirmed)
                {
                    return new ResourceValidationResult
                    {
                        IsValid = false,
                        AuthorizationPoliciesResult = new[]
                        {
                            new AuthorizationPolicyResult
                            {
                                Type = AuthorizationPolicyResultEnum.RequestNotConfirmed,
                                Policy = vr.Policy
                            }
                        }
                    };
                }
            }

            return new ResourceValidationResult
            {
                IsValid = true
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

        private async Task<AuthorizationPolicyResult> CheckClaims(string openidProvider, Policy authorizationPolicy, ClaimTokenParameter claimTokenParameter)
        {
            if (authorizationPolicy.Claims == null || !authorizationPolicy.Claims.Any())
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized,
                    Policy = authorizationPolicy
                };
            }

            if (claimTokenParameter == null || claimTokenParameter.Format != Constants.IdTokenType)
            {
                var tmp = GetNeedInfoResult(authorizationPolicy.Claims, openidProvider);
                tmp.Policy = authorizationPolicy;
                return tmp;
            }

            var idToken = claimTokenParameter.Token;
            var jwsPayload = await _jwtTokenParser.UnSign(idToken, openidProvider, authorizationPolicy).ConfigureAwait(false);
            if (jwsPayload == null)
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized,
                    Policy = authorizationPolicy
                };
            }
            
            foreach (var claim in authorizationPolicy.Claims)
            {
                var payload = jwsPayload.FirstOrDefault(j => j.Key == claim.Type);
                if (payload.Equals(default(KeyValuePair<string, object>)))
                {
                    return new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.NotAuthorized
                    };
                }

                if (claim.Type == SimpleIdServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role)
                {
                    IEnumerable<string> roles = null;
                    if (payload.Value is string)
                    {
                        roles = payload.Value.ToString().Split(',');
                    }
                    else
                    {
                        var arr = payload.Value as object[];
                        var jArr = payload.Value as JArray;
                        if (arr != null)
                        {
                            roles = arr.Select(c => c.ToString());
                        }

                        if (jArr != null)
                        {
                            roles = jArr.Select(c => c.ToString());
                        }
                    }

                    if (roles == null || !roles.Any(v => claim.Value == v))
                    {
                        return new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.NotAuthorized
                        };
                    }
                }
                else
                {
                    if (payload.Value.ToString() != claim.Value)
                    {
                        return new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.NotAuthorized
                        };
                    }
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
                Subject = subject,
                Policy = authorizationPolicy
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
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
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
