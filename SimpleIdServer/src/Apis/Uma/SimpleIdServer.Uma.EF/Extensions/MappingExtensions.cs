using SimpleIdServer.Uma.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain = SimpleIdServer.Uma.Core.Models;

namespace SimpleIdServer.Uma.EF.Extensions
{
    internal static class MappingExtensions
    {
        public static Domain.SharedLink ToDomain(this ShareResourceLink shareResourceLink)
        {
            return new Domain.SharedLink
            {
                ConfirmationCode = shareResourceLink.ConfirmationCode,
                ResourceId = shareResourceLink.ResourceId,
                Scopes = GetList(shareResourceLink.Scopes)
            };
        }

        public static Domain.ResourceSet ToDomain(this ResourceSet resourceSet)
        {
            var policyIds = resourceSet.PolicyResources != null ?
                resourceSet.PolicyResources.Select(p => p.PolicyId)
                .ToList() : new List<string>();
            var policies = resourceSet.PolicyResources == null ? new Domain.Policy[0] :
                resourceSet.PolicyResources.Where(p => p.Policy != null).Select(p => p.Policy.ToDomain());
            return new Domain.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes.Select(s => s.Scope).ToList(),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                Owner = resourceSet.Owner,
                AuthorizationPolicyIds = policyIds,
                Policies = policies
            };
        }

        public static Domain.Policy ToDomain(this Policy policy)
        {
            var rules = new List<Domain.PolicyRule>();
            var resourceSetIds = new List<string>();
            if (policy.Rules != null)
            {
                rules = policy.Rules.Select(r => r.ToDomain()).ToList();
            }

            if (policy.PolicyResources != null)
            {
                resourceSetIds = policy.PolicyResources
                    .Select(r => r.ResourceSetId)
                    .ToList();
            }

            return new Domain.Policy
            {
                Id = policy.Id,
                Rules = rules,
                ResourceSetIds = resourceSetIds
            };
        }

        public static Domain.PolicyRule ToDomain(this PolicyRule policyRule)
        {
            var claims = new List<Domain.Claim>();
            if (policyRule.Claims != null &&
                policyRule.Claims.Any())
            {
                claims = policyRule.Claims.Select(c => new Domain.Claim
                {
                    Type = c.Key,
                    Value = c.Value
                }).ToList();
            }

            return new Domain.PolicyRule
            {
                Id = policyRule.Id,
                ClientIdsAllowed = policyRule.ClientIdsAllowed.Select(c => c.ClientId).ToList(),
                Scopes = policyRule.Scopes.Select(s => s.Scope).ToList(),
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Script = policyRule.Script,
                Claims = claims,
                OpenIdProvider = policyRule.OpenIdProvider
            };
        }

        public static ShareResourceLink ToModel(this Domain.SharedLink shareResourceLink)
        {
            return new ShareResourceLink
            {
                ConfirmationCode = shareResourceLink.ConfirmationCode,
                ResourceId = shareResourceLink.ResourceId,
                Scopes = GetConcatenatedList(shareResourceLink.Scopes)
            };
        }

        public static ResourceSet ToModel(this Domain.ResourceSet resourceSet)
        {
            var policyIds = resourceSet.AuthorizationPolicyIds != null ?
                resourceSet.AuthorizationPolicyIds.Select(p =>
                    new PolicyResource
                    {
                        ResourceSetId = resourceSet.Id,
                        PolicyId = p
                    }).ToList()
                : new List<PolicyResource>();
            return new ResourceSet
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes == null ? new List<ResourceScope>() : resourceSet.Scopes.Select(s => new ResourceScope
                {
                    ResourceId = resourceSet.Id,
                    Scope = s
                }).ToList(),
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                Owner = resourceSet.Owner,
                PolicyResources = policyIds
            };
        }

        public static Policy ToModel(this Domain.Policy policy)
        {
            var rules = new List<PolicyRule>();
            var resources = new List<PolicyResource>();
            if (policy.Rules != null)
            {
                rules = policy.Rules.Select(r => r.ToModel()).ToList();
            }

            if (policy.ResourceSetIds != null)
            {
                resources = policy.ResourceSetIds.Select(r => new PolicyResource
                {
                    PolicyId = policy.Id,
                    ResourceSetId = r
                }).ToList();
            }

            return new Policy
            {
                Id = policy.Id,
                Rules = rules,
                PolicyResources = resources
            };
        }

        public static PolicyRule ToModel(this Domain.PolicyRule policyRule)
        {
            var claims = new List<PolicyRuleClaim>();
            if (policyRule.Claims != null && 
                policyRule.Claims.Any())
            {
                claims = policyRule.Claims.Select(c =>
                new PolicyRuleClaim
                {
                    Key = c.Type,
                    Value = c.Value,
                    PolicyRuleId = policyRule.Id
                }).ToList();
            }

            return new PolicyRule
            {
                Id = policyRule.Id,
                ClientIdsAllowed = policyRule.ClientIdsAllowed == null ? new List<PolicyRuleClientId>() : policyRule.ClientIdsAllowed.Select(c =>
                    new PolicyRuleClientId
                    {
                        ClientId = c,
                        PolicyRuleId = policyRule.Id
                    }).ToList(),
                Scopes = policyRule.Scopes == null ? new List<PolicyRuleScope>() : policyRule.Scopes.Select(s =>
                    new PolicyRuleScope
                    {
                        PolicyRuleId = policyRule.Id,
                        Scope = s
                    }).ToList(),
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Script = policyRule.Script,
                Claims = claims,
                OpenIdProvider=  policyRule.OpenIdProvider
            };
        }

        public static List<string> GetList(string concatenatedList)
        {

            var scopes = new List<string>();
            if (!string.IsNullOrEmpty(concatenatedList))
            {
                scopes = concatenatedList.Split(',').ToList();
            }

            return scopes;
        }

        public static string GetConcatenatedList(IEnumerable<string> list)
        {
            var concatenatedList = string.Empty;
            if (list != null && list.Any())
            {
                concatenatedList = string.Join(",", list);
            }

            return concatenatedList;
        }
    }
}
