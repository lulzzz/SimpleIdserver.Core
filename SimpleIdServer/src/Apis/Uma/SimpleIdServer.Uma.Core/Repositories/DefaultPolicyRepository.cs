using SimpleIdServer.Uma.Core.Extensions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    internal sealed class DefaultPolicyRepository : IPolicyRepository
    {
        private static object _obj = new object();

        public DefaultPolicyRepository(ICollection<Policy> policies)
        {
            Policies = policies == null ? new List<Policy>() : policies;
        }

        public static ICollection<Policy> Policies;

        public Task<bool> Add(Policy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            Policies.Add(policy.Copy());
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string id)
        {
            lock(_obj)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentNullException(nameof(id));
                }

                var policy = Policies.FirstOrDefault(p => p.Id == id);
                if (policy == null)
                {
                    return Task.FromResult(false);
                }

                Policies.Remove(policy);
                return Task.FromResult(true);
            }
        }

        public Task<Policy> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var r = Policies.FirstOrDefault(p => p.Id == id);
            if (r == null)
            {
                return Task.FromResult((Policy)null);
            }

            return Task.FromResult(r.Copy());
        }

        public Task<ICollection<Policy>> GetAll()
        {
            ICollection<Policy> result = Policies.Select(p => p.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter)
        {
            if(parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<Policy> result = Policies;
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.ResourceIds != null && parameter.ResourceIds.Any())
            {
                result = result.Where(p => p.ResourceSetIds.Any(r => parameter.ResourceIds.Contains(r)));
            }

            var nbResult = result.Count();
            result = result.OrderBy(c => c.Id);
            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchAuthPoliciesResult
            {
                Content = result.Select(r => r.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<ICollection<Policy>> SearchByResourceId(string resourceSetId)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            ICollection<Policy> result = Policies.Where(p => p.ResourceSetIds.Contains(resourceSetId))
                .Select(r => r.Copy())
                .ToList();
            return Task.FromResult(result);
        }

        public Task<bool> Update(Policy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var rec = Policies.FirstOrDefault(p => p.Id == policy.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.ResourceSetIds = policy.ResourceSetIds;
            rec.Scopes = policy.Scopes;
            rec.Script = policy.Script;
            rec.IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded;
            rec.Claims = policy.Claims;
            return Task.FromResult(true);
        }
    }
}
