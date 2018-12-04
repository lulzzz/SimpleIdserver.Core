using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.EF.Repositories
{
    internal class PolicyRepository : IPolicyRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public PolicyRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Policy>> GetAll()
        {
            return await _context.Policies.Select(p => p.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Policy> Get(string id)
        {
            var policy = await _context.Policies
                .Include(p => p.ResourceSetPolicies)
                .Include(p => p.Scopes)
                .Include(p => p.Claims)
                .Include(p => p.Clients)
                .FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
            return policy == null ? null : policy.ToDomain();
        }

        public async Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Models.Policy> policies = _context.Policies
                .Include(p => p.ResourceSetPolicies)
                .Include(p => p.Scopes)
                .Include(p => p.Claims)
                .Include(p => p.Clients);
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                policies = policies.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.ResourceIds != null && parameter.ResourceIds.Any())
            {
                policies = policies.Where(p => p.ResourceSetPolicies.Any(r => parameter.ResourceIds.Contains(r.ResourceSetId)));
            }

            var nbResult = await policies.CountAsync().ConfigureAwait(false);
            policies = policies.OrderBy(c => c.Id);
            if (parameter.IsPagingEnabled)
            {
                policies = policies.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return new SearchAuthPoliciesResult
            {
                Content = await policies.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            };
        }

        public async Task<bool> Add(Policy policy)
        {
            _context.Policies.Add(policy.ToModel());
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Delete(string id)
        {
            var policy = await _context.Policies
                .Include(p => p.Scopes)
                .Include(p => p.Claims)
                .Include(p => p.Clients)
                .FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
            if (policy == null)
            {
                return false;
            }

            _context.Policies.Remove(policy);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Update(Policy policy)
        {
            var record = await _context.Policies
                .Include(p => p.Clients)
                .Include(p => p.Scopes)
                .Include(p => p.Claims)
                .Include(p => p.ResourceSetPolicies)
                .FirstOrDefaultAsync(p => p.Id == policy.Id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            // Update scopes
            var scopesNotToBeDeleted = new List<string>();
            if (policy.Scopes != null)
            {
                foreach (var scope in policy.Scopes)
                {
                    var s = record.Scopes.FirstOrDefault(c => c.Scope == scope);
                    if (s == null)
                    {
                        s = new Models.PolicyScope
                        {
                            PolicyId = record.Id,
                            Scope = scope
                        };
                        record.Scopes.Add(s);
                    }

                    scopesNotToBeDeleted.Add(scope);
                }
            }

            var scopeNames = record.Scopes.Select(o => o.Scope).ToList();
            foreach (var scopeName in scopeNames.Where(id => !scopesNotToBeDeleted.Contains(id)))
            {
                record.Scopes.Remove(record.Scopes.First(s => s.Scope == scopeName));
            }

            // Update claims
            var claimsNotToBeDeleted = new List<string>();
            if (policy.Claims != null)
            {
                foreach (var claim in policy.Claims)
                {
                    var s = record.Claims.FirstOrDefault(c => c.Key == claim.Type);
                    if (s == null)
                    {
                        s = new Models.PolicyClaim
                        {
                            PolicyId = record.Id,
                            Key = claim.Type,
                            Value = claim.Value
                        };
                        record.Claims.Add(s);
                    }
                    else
                    {
                        s.Value = claim.Value;
                    }

                    claimsNotToBeDeleted.Add(claim.Type);
                }
            }

            var claimKeys = record.Claims.Select(o => o.Key).ToList();
            foreach (var claimKey in claimKeys.Where(id => !claimsNotToBeDeleted.Contains(id)))
            {
                record.Claims.Remove(record.Claims.First(s => s.Key == claimKey));
            }

            // Update clients
            var clientsNotToBeDeleted = new List<string>();
            if (policy.ClientIds != null)
            {
                foreach (var clientId in policy.ClientIds)
                {
                    var s = record.Clients.FirstOrDefault(c => c.ClientId == clientId);
                    if (s == null)
                    {
                        s = new Models.PolicyClient
                        {
                            PolicyId = record.Id,
                            ClientId = clientId
                        };
                        record.Clients.Add(s);
                    }

                    clientsNotToBeDeleted.Add(clientId);
                }
            }

            var clientIds = record.Clients.Select(o => o.ClientId).ToList();
            foreach (var clientId in clientIds.Where(id => !clientsNotToBeDeleted.Contains(id)))
            {
                record.Clients.Remove(record.Clients.First(s => s.ClientId == clientId));
            }

            // Update resources.
            var resourceSetIdsNotToBeDeleted = new List<string>();
            if (policy.ResourceSetIds != null)
            {
                foreach (var resourceSetId in policy.ResourceSetIds)
                {
                    var policyResource = record.ResourceSetPolicies.FirstOrDefault(p => p.ResourceSetId == resourceSetId);
                    if (policyResource == null)
                    {
                        policyResource = new Models.ResourceSetPolicy
                        {
                            ResourceSetId = resourceSetId,
                            PolicyId = policy.Id
                        };
                        record.ResourceSetPolicies.Add(policyResource);
                    }

                    resourceSetIdsNotToBeDeleted.Add(policyResource.ResourceSetId);
                }
            }

            var resourceSetIds = record.ResourceSetPolicies.Select(o => o.ResourceSetId).ToList();
            foreach (var resourceSetId in resourceSetIds.Where(id => !resourceSetIdsNotToBeDeleted.Contains(id)))
            {
                var removedResource = record.ResourceSetPolicies.First(o => o.ResourceSetId == resourceSetId);
                record.ResourceSetPolicies.Remove(removedResource);
            }

            record.IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded;
            record.Script = policy.Script;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<ICollection<Policy>> SearchByResourceId(string resourceSetId)
        {
            return await _context.Policies
                .Include(p => p.ResourceSetPolicies)
                .Include(p => p.Scopes)
                .Include(p => p.Claims)
                .Include(p => p.Clients)
                .Where(p => p.ResourceSetPolicies.Any(r => r.ResourceSetId == resourceSetId))
                .Select(p => p.ToDomain())
                .ToListAsync();
        }
    }
}