using SimpleIdServer.Uma.EF.Models;
using System.Collections.Generic;
using System.Linq;
using Domain = SimpleIdServer.Uma.Core.Models;

namespace SimpleIdServer.Uma.EF.Extensions
{
    internal static class MappingExtensions
    {
        public static Domain.PendingRequest ToDomain(this ResourcePendingRequest pendingRequest)
        {
            return new Domain.PendingRequest
            {
                ResourceId = pendingRequest.ResourceId,
                CreateDateTime = pendingRequest.CreateDateTime,
                IsConfirmed = pendingRequest.IsConfirmed,
                RequesterSubject = pendingRequest.RequesterSubject,
                Scopes = pendingRequest.Scopes == null ? new List<string>() : pendingRequest.Scopes.Select(s => s.Scope),
                Resource = pendingRequest.Resource == null ? null : pendingRequest.Resource.ToDomain()
            };
        }

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
            var policyIds = resourceSet.ResourceSetPolicies != null ?
                resourceSet.ResourceSetPolicies.Select(p => p.PolicyId)
                .ToList() : new List<string>();
            var policies = resourceSet.ResourceSetPolicies == null ? new Domain.Policy[0] :
                resourceSet.ResourceSetPolicies.Where(p => p.Policy != null).Select(p => p.Policy.ToDomain());
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
                AcceptPendingRequest = resourceSet.AcceptPendingRequest,
                AuthPolicies = policies
            };
        }

        public static Domain.Policy ToDomain(this Policy policy)
        {
            var resourceSetIds = new List<string>();
            var claims = new List<Domain.Claim>();
            if (policy.ResourceSetPolicies != null)
            {
                resourceSetIds = policy.ResourceSetPolicies
                    .Select(r => r.ResourceSetId)
                    .ToList();
            }

            if (policy.Claims != null && policy.Claims.Any())
            {
                claims = policy.Claims.Select(c => new Domain.Claim
                {
                    Type = c.Key,
                    Value = c.Value
                }).ToList();
            }

            return new Domain.Policy
            {
                Id = policy.Id,
                Claims = claims,
                ClientIds = policy.Clients.Select(s => s.ClientId).ToList(),
                Script = policy.Script,
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                Scopes = policy.Scopes.Select(s => s.Scope).ToList(),
                ResourceSetIds = resourceSetIds
            };
        }

        public static ResourcePendingRequest ToModel(this Domain.PendingRequest pendingRequest)
        {
            return new ResourcePendingRequest
            {
                Id = pendingRequest.Id,
                CreateDateTime = pendingRequest.CreateDateTime,
                IsConfirmed = pendingRequest.IsConfirmed,
                Scopes = pendingRequest.Scopes == null ? new List<PendingRequestScope>() : pendingRequest.Scopes.Select(s => new PendingRequestScope
                {
                    PendingRequestId = pendingRequest.Id,
                    Scope = s
                }).ToList(),
                ResourceId = pendingRequest.ResourceId,
                RequesterSubject = pendingRequest.RequesterSubject
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
                    new ResourceSetPolicy
                    {
                        ResourceSetId = resourceSet.Id,
                        PolicyId = p
                    }).ToList()
                : new List<ResourceSetPolicy>();
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
                AcceptPendingRequest = resourceSet.AcceptPendingRequest,
                Owner = resourceSet.Owner,
                ResourceSetPolicies = policyIds
            };
        }

        public static Policy ToModel(this Domain.Policy policy)
        {
            var resources = new List<ResourceSetPolicy>();
            var claims = new List<PolicyClaim>();
            if (policy.Claims != null && policy.Claims.Any())
            {
                claims = policy.Claims.Select(c =>
                new PolicyClaim
                {
                    Key = c.Type,
                    Value = c.Value,
                    PolicyId = policy.Id
                }).ToList();
            }

            if (policy.ResourceSetIds != null)
            {
                resources = policy.ResourceSetIds.Select(r => new ResourceSetPolicy
                {
                    PolicyId = policy.Id,
                    ResourceSetId = r
                }).ToList();
            }

            return new Policy
            {
                Id = policy.Id,
                Claims = claims,
                Clients = policy.ClientIds == null ? new List<PolicyClient>() : policy.ClientIds.Select(c => new PolicyClient
                {
                    ClientId = c,
                    PolicyId =  policy.Id
                }).ToList(),
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                Script = policy.Script,
                Scopes = policy.Scopes == null ? new List<PolicyScope>() : policy.Scopes.Select(s =>
                    new PolicyScope
                    {
                        PolicyId = policy.Id,
                        Scope = s
                    }).ToList(),
                ResourceSetPolicies = resources
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
