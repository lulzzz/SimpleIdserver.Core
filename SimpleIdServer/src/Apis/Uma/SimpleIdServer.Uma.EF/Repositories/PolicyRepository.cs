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
                .Include(p => p.PolicyResources)
                .Include(p => p.Rules).ThenInclude(p => p.Scopes)
                .Include(p => p.Rules).ThenInclude(p => p.ClientIdsAllowed)
                .Include(p => p.Rules).ThenInclude(p => p.Claims)
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
                .Include(p => p.PolicyResources)
                .Include(p => p.Rules).ThenInclude(p => p.Scopes)
                .Include(p => p.Rules).ThenInclude(p => p.ClientIdsAllowed)
                .Include(p => p.Rules).ThenInclude(p => p.Claims);
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                policies = policies.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.ResourceIds != null && parameter.ResourceIds.Any())
            {
                policies = policies.Where(p => p.PolicyResources.Any(r => parameter.ResourceIds.Contains(r.ResourceSetId)));
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
                .Include(p => p.Rules).ThenInclude(p => p.Scopes)
                .Include(p => p.Rules).ThenInclude(p => p.ClientIdsAllowed)
                .Include(p => p.Rules).ThenInclude(p => p.Claims)
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
            var record = await _context.Policies.FirstOrDefaultAsync(p => p.Id == policy.Id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            var rulesNotToBeDeleted = new List<string>();
            if (policy.Rules != null)
            {
                foreach (var ru in policy.Rules)
                {
                    var rule = record.Rules.FirstOrDefault(r => r.Id == ru.Id);
                    if (rule == null)
                    {
                        rule = new Models.PolicyRule
                        {
                            Id = Guid.NewGuid().ToString(),
                            PolicyId = policy.Id
                        };
                        record.Rules.Add(rule);
                    }

                    rule.IsResourceOwnerConsentNeeded = ru.IsResourceOwnerConsentNeeded;
                    rule.Script = ru.Script;
                    rule.OpenIdProvider = ru.OpenIdProvider;
                    rule.ClientIdsAllowed = ru.ClientIdsAllowed == null ? new List<Models.PolicyRuleClientId>() : ru.ClientIdsAllowed.Select(c => new Models.PolicyRuleClientId
                    {
                        ClientId = c,
                        PolicyRuleId = rule.Id
                    }).ToList();
                    rule.Scopes = ru.Scopes == null ? new List<Models.PolicyRuleScope>() : ru.ClientIdsAllowed.Select(c => new Models.PolicyRuleScope
                    {
                        Scope = c,
                        PolicyRuleId = rule.Id
                    }).ToList();
                    rule.Claims = ru.Claims == null ? new List<Models.PolicyRuleClaim>() : ru.Claims.Select(c => new Models.PolicyRuleClaim
                    {
                        Key = c.Type,
                        Value = c.Value,
                        PolicyRuleId = rule.Id
                    }).ToList();
                    rulesNotToBeDeleted.Add(rule.Id);
                }
            }

            var ruleIds = record.Rules.Select(o => o.Id).ToList();
            foreach (var ruleId in ruleIds.Where(id => !rulesNotToBeDeleted.Contains(id)))
            {
                var removedRule = record.Rules.First(o => o.Id == ruleId);
                record.Rules.Remove(removedRule);
                _context.PolicyRules.Remove(removedRule);
            }

            var resourceSetIdsNotToBeDeleted = new List<string>();
            if (policy.ResourceSetIds != null)
            {
                foreach (var resourceSetId in policy.ResourceSetIds)
                {
                    var policyResource = record.PolicyResources.FirstOrDefault(p => p.ResourceSetId == resourceSetId);
                    if (policyResource == null)
                    {
                        policyResource = new Models.PolicyResource
                        {
                            ResourceSetId = resourceSetId,
                            PolicyId = policy.Id
                        };
                        record.PolicyResources.Add(policyResource);
                    }

                    resourceSetIdsNotToBeDeleted.Add(policyResource.ResourceSetId);
                }
            }

            var resourceSetIds = record.PolicyResources.Select(o => o.ResourceSetId).ToList();
            foreach (var resourceSetId in resourceSetIds.Where(id => !resourceSetIdsNotToBeDeleted.Contains(id)))
            {
                var removedResource = record.PolicyResources.First(o => o.ResourceSetId == resourceSetId);
                record.PolicyResources.Remove(removedResource);
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<ICollection<Policy>> SearchByResourceId(string resourceSetId)
        {
            return await _context.Policies
                .Include(p => p.PolicyResources)
                .Where(p => p.PolicyResources.Any(r => r.ResourceSetId == resourceSetId))
                .Select(p => p.ToDomain())
                .ToListAsync();
        }
    }
}