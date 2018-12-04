using SimpleIdServer.Uma.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Uma.Core.Extensions
{
    public static class CopyExtensions
    {
        public static ResourceSet Copy(this ResourceSet resourceSet)
        {
            return new ResourceSet
            {
                AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds == null ? new List<string>() : resourceSet.AuthorizationPolicyIds.ToList(),
                IconUri = resourceSet.IconUri,
                Id = resourceSet.Id,
                Name = resourceSet.Name,
                Type = resourceSet.Type,
                Owner = resourceSet.Owner,
                Uri = resourceSet.Uri,
                AcceptPendingRequest = resourceSet.AcceptPendingRequest,
                AuthPolicies = resourceSet.AuthPolicies == null ? new List<Policy>() : resourceSet.AuthPolicies.Select(p => p.Copy()).ToList(),
                Scopes = resourceSet.Scopes == null ? new List<string>() : resourceSet.Scopes.ToList()
            };
        }

        public static Policy Copy(this Policy policy)
        {
            return new Policy
            {
                Id = policy.Id,
                Scopes = policy.Scopes == null ? new List<string>() : policy.Scopes.ToList(),
                Script = policy.Script,
                ClientIds = policy.ClientIds == null ? new List<string>() : policy.ClientIds.ToList(),
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                ResourceSetIds = policy.ResourceSetIds.ToList(),
                Claims = policy.Claims == null ? new List<Claim>() : policy.Claims.Select(c =>
                    new Claim
                    {
                        Type = c.Type,
                        Value = c.Value
                    }
                ).ToList()
            };
        }
    }
}