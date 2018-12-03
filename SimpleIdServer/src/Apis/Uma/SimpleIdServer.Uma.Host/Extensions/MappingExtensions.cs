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

using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Dtos.Requests;
using SimpleIdServer.Dtos.Responses;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Uma.Host.Extensions
{
    internal static class MappingExtensions
    {
        #region UMA

        public static SearchResourceSetParameter ToParameter(this SearchResourceSet searchResourceSet)
        {
            if (searchResourceSet == null)
            {
                throw new ArgumentNullException(nameof(searchResourceSet));
            }

            return new SearchResourceSetParameter
            {
                Count = searchResourceSet.TotalResults,
                Ids = searchResourceSet.Ids,
                Names = searchResourceSet.Names,
                Owners = searchResourceSet.Owners,
                StartIndex = searchResourceSet.StartIndex,
                Types = searchResourceSet.Types
            };
        }

        public static SearchAuthPoliciesParameter ToParameter(this SearchAuthPolicies searchAuthPolicies)
        {
            if (searchAuthPolicies == null)
            {
                throw new ArgumentNullException(nameof(searchAuthPolicies));
            }

            return new SearchAuthPoliciesParameter
            {
                Count = searchAuthPolicies.TotalResults,
                Ids = searchAuthPolicies.Ids,
                StartIndex = searchAuthPolicies.StartIndex,
                ResourceIds = searchAuthPolicies.ResourceIds
            };
        }

        public static AddResouceSetParameter ToParameter(this PostResourceSet postResourceSet)
        {
            return new AddResouceSetParameter
            {
                IconUri = postResourceSet.IconUri,
                Name = postResourceSet.Name,
                Scopes = postResourceSet.Scopes,
                Type = postResourceSet.Type,
                Uri = postResourceSet.Uri,
                Owner = postResourceSet.Owner
            };
        }

        public static UpdateResourceSetParameter ToParameter(this PutResourceSet putResourceSet)
        {
            return new UpdateResourceSetParameter
            {
                Id = putResourceSet.Id,
                Name = putResourceSet.Name,
                IconUri = putResourceSet.IconUri,
                Scopes = putResourceSet.Scopes,
                Type = putResourceSet.Type,
                Uri = putResourceSet.Uri,
                Owner = putResourceSet.Owner
            };
        }

        public static AddScopeParameter ToParameter(this PostScope postScope)
        {
            return new AddScopeParameter
            {
                Id = postScope.Id,
                Name = postScope.Name,
                IconUri = postScope.IconUri
            };
        }

        public static UpdateScopeParameter ToParameter(this PutScope putScope)
        {
            return new UpdateScopeParameter
            {
                Id = putScope.Id,
                Name = putScope.Name,
                IconUri = putScope.IconUri
            };
        }

        public static AddPermissionParameter ToParameter(this PostPermission postPermission)
        {
            return new AddPermissionParameter
            {
                ResourceSetId = postPermission.ResourceSetId,
                Scopes = postPermission.Scopes
            };
        }

        public static GetAuthorizationActionParameter ToParameter(this PostAuthorization postAuthorization)
        {
            var tokens = new List<ClaimTokenParameter>();
            if (postAuthorization.ClaimTokens != null &&
                postAuthorization.ClaimTokens.Any())
            {
                tokens = postAuthorization.ClaimTokens.Select(ct => ct.ToParameter()).ToList();
            }

            return new GetAuthorizationActionParameter
            {
                Rpt = postAuthorization.Rpt,
                TicketId = postAuthorization.TicketId,
                ClaimTokenParameters = tokens
            };
        }

        public static ClaimTokenParameter ToParameter(this PostClaimToken postClaimToken)
        {
            return new ClaimTokenParameter
            {
                Format = postClaimToken.Format,
                Token = postClaimToken.Token
            };
        }

        public static AddPolicyParameter ToParameter(this PostPolicy postPolicy)
        {
            return new AddPolicyParameter
            {
                Claims = postPolicy.Claims == null ? new List<AddClaimParameter>() : postPolicy.Claims.Select(c => c.ToParameter()).ToList(),
                IsResourceOwnerConsentNeeded = postPolicy.IsResourceOwnerConsentNeeded,
                Script = postPolicy.Script,
                ResourceSetIds = postPolicy.ResourceSetIds,
                Scopes = postPolicy.Scopes,
                ClientIdsAllowed = postPolicy.ClientIdsAllowed
            };
        }

        public static AddClaimParameter ToParameter(this PostClaim postClaim)
        {
            return new AddClaimParameter
            {
                Type = postClaim.Type,
                Value = postClaim.Value
            };
        }

        public static UpdatePolicyParameter ToParameter(this PutPolicy putPolicy)
        {
            return new UpdatePolicyParameter
            {
                PolicyId = putPolicy.PolicyId,
                Claims = putPolicy.Claims == null ? new List<AddClaimParameter>() : putPolicy.Claims.Select(c => c.ToParameter()).ToList(),
                IsResourceOwnerConsentNeeded = putPolicy.IsResourceOwnerConsentNeeded,
                Script = putPolicy.Script,
                Scopes = putPolicy.Scopes,
                ClientIdsAllowed = putPolicy.ClientIdsAllowed
            };
        }

        public static SearchResourceSetResponse ToResponse(this SearchResourceSetResult searchResourceSetResult)
        {
            if (searchResourceSetResult == null)
            {
                throw new ArgumentNullException(nameof(searchResourceSetResult));
            }

            return new SearchResourceSetResponse
            {
                StartIndex = searchResourceSetResult.StartIndex,
                TotalResults = searchResourceSetResult.TotalResults,
                Content = searchResourceSetResult.Content == null ? new List<ResourceSetResponse>() : searchResourceSetResult.Content.Select(s => s.ToResponse())
            };
        }

        public static SearchAuthPoliciesResponse ToResponse(this SearchAuthPoliciesResult searchAuthPoliciesResult)
        {
            if (searchAuthPoliciesResult == null)
            {
                throw new ArgumentNullException(nameof(searchAuthPoliciesResult));
            }

            return new SearchAuthPoliciesResponse
            {
                StartIndex = searchAuthPoliciesResult.StartIndex,
                TotalResults = searchAuthPoliciesResult.TotalResults,
                Content = searchAuthPoliciesResult.Content == null ? new List<PolicyResponse>() : searchAuthPoliciesResult.Content.Select(s => s.ToResponse())
            };
        }

        public static ResourceSetResponse ToResponse(this ResourceSet resourceSet)
        {
            return new ResourceSetResponse
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri ,
                Owner = resourceSet.Owner
            };
        }

        public static PolicyResponse ToResponse(this Policy policy)
        {
            return new PolicyResponse
            {
                Id = policy.Id,
                ResourceSetIds = policy.ResourceSetIds,
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                Script = policy.Script,
                Scopes = policy.Scopes,
                Claims = policy.Claims == null ? new List<PostClaim>() : policy.Claims.Select(c => new PostClaim
                {
                    Type = c.Type,
                    Value = c.Value
                })
            };
        }

        public static PostClaim ToResponse(this Claim claim)
        {
            return new PostClaim
            {
                Type = claim.Type,
                Value = claim.Value
            };
        }

        #endregion

        #region OAUTH2.0
        
        public static GrantedTokenResponse ToDto(this GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            return new GrantedTokenResponse
            {
                AccessToken = grantedToken.AccessToken,
                IdToken = grantedToken.IdToken,
                ExpiresIn = grantedToken.ExpiresIn,
                RefreshToken = grantedToken.RefreshToken,
                TokenType = grantedToken.TokenType,
                Scope = grantedToken.Scope.Split(' ').ToList()
            };
        }

        public static RegistrationParameter ToParameter(this ClientRequest clientRequest)
        {
            if (clientRequest == null)
            {
                throw new ArgumentNullException(nameof(clientRequest));
            }

            var responseTypes = new List<ResponseType>();
            var grantTypes = new List<GrantType>();
            ApplicationTypes? applicationType = null;
            if (clientRequest.ResponseTypes != null && clientRequest.ResponseTypes.Any())
            {
                foreach (var responseType in clientRequest.ResponseTypes)
                {
                    ResponseType responseTypeEnum;
                    if (Enum.TryParse(responseType, out responseTypeEnum) &&
                        !responseTypes.Contains(responseTypeEnum))
                    {
                        responseTypes.Add(responseTypeEnum);
                    }
                }
            }

            if (clientRequest.GrantTypes != null && clientRequest.GrantTypes.Any())
            {
                foreach (var grantType in clientRequest.GrantTypes)
                {
                    GrantType grantTypeEnum;
                    if (Enum.TryParse(grantType, out grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            ApplicationTypes appTypeEnum;
            if (Enum.TryParse(clientRequest.ApplicationType, out appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            return new RegistrationParameter
            {
                RequirePkce = clientRequest.RequirePkce,
                ApplicationType = applicationType,
                ClientName = clientRequest.ClientName,
                ClientUri = clientRequest.ClientUri,
                Contacts = clientRequest.Contacts == null ? new List<string>() : clientRequest.Contacts.ToList(),
                DefaultAcrValues = clientRequest.DefaultAcrValues,
                DefaultMaxAge = clientRequest.DefaultMaxAge,
                GrantTypes = grantTypes,
                IdTokenEncryptedResponseAlg = clientRequest.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientRequest.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientRequest.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientRequest.InitiateLoginUri,
                Jwks = clientRequest.Jwks,
                JwksUri = clientRequest.JwksUri,
                LogoUri = clientRequest.LogoUri,
                PolicyUri = clientRequest.PolicyUri,
                PostLogoutRedirectUris = clientRequest.PostLogoutRedirectUris == null ? new List<string>() : clientRequest.PostLogoutRedirectUris.ToList(),
                RedirectUris = clientRequest.RedirectUris == null ? new List<string>() : clientRequest.RedirectUris.ToList(),
                RequestObjectEncryptionAlg = clientRequest.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientRequest.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = clientRequest.RequestObjectSigningAlg,
                RequestUris = clientRequest.RequestUris == null ? new List<string>() : clientRequest.RequestUris.ToList(),
                RequireAuthTime = clientRequest.RequireAuthTime,
                ResponseTypes = responseTypes,
                SectorIdentifierUri = clientRequest.SectorIdentifierUri,
                SubjectType = clientRequest.SubjectType,
                TokenEndPointAuthMethod = clientRequest.TokenEndpointAuthMethod,
                TokenEndPointAuthSigningAlg = clientRequest.TokenEndpointAuthSigningAlg,
                TosUri = clientRequest.TosUri,
                UserInfoEncryptedResponseAlg = clientRequest.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = clientRequest.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = clientRequest.UserInfoSignedResponseAlg,
                ScimProfile = clientRequest.ScimProfile
            };
        }


        public static SimpleIdServer.Dtos.Responses.IntrospectionResponse ToDto(this IntrospectionResult introspectionResult)
        {
            return new SimpleIdServer.Dtos.Responses.IntrospectionResponse
            {
                Active = introspectionResult.Active,
                Audience = introspectionResult.Audience,
                ClientId = introspectionResult.ClientId,
                Expiration = introspectionResult.Expiration,
                IssuedAt = introspectionResult.IssuedAt,
                Issuer = introspectionResult.Issuer,
                Jti = introspectionResult.Jti,
                Nbf = introspectionResult.Nbf,
                Scope = introspectionResult.Scope.Split(' ').ToList(),
                Subject = introspectionResult.Subject,
                TokenType = introspectionResult.TokenType,
                UserName = introspectionResult.UserName
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                UserName = request.Username,
                Password = request.Password,
                Scope = request.Scope,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Code = request.Code,
                RedirectUri = request.RedirectUri,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                CodeVerifier = request.CodeVerifier
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                RefreshToken = request.RefreshToken
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };
        }

        public static GetTokenViaTicketIdParameter ToTokenIdGrantTypeParameter(this TokenRequest request)
        {
            return new GetTokenViaTicketIdParameter
            {
                ClaimToken = request.ClaimToken,
                ClaimTokenFormat = request.ClaimTokenFormat,
                Pct = request.Pct,
                Rpt = request.Rpt,
                Ticket = request.Ticket
            };
        }
        
        public static IntrospectionParameter ToParameter(this IntrospectionRequest viewModel)
        {
            return new IntrospectionParameter
            {
                ClientAssertion = viewModel.ClientAssertion,
                ClientAssertionType = viewModel.ClientAssertionType,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret,
                Token = viewModel.Token,
                TokenTypeHint = viewModel.TokenTypeHint
            };
        }

        public static RevokeTokenParameter ToParameter(this RevocationRequest revocationRequest)
        {
            return new RevokeTokenParameter
            {
                ClientAssertion = revocationRequest.ClientAssertion,
                ClientAssertionType = revocationRequest.ClientAssertionType,
                ClientId = revocationRequest.ClientId,
                ClientSecret = revocationRequest.ClientSecret,
                Token = revocationRequest.Token,
                TokenTypeHint = revocationRequest.TokenTypeHint
            };
        }

        #endregion
    }
}
