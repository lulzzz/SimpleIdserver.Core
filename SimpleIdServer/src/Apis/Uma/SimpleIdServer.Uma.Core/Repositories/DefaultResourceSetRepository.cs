using SimpleIdServer.Uma.Core.Extensions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    internal sealed class DefaultResourceSetRepository : IResourceSetRepository
    {
        private static object _obj = new object();

        public DefaultResourceSetRepository(ICollection<ResourceSet> resources)
        {
            Resources = resources == null ? new List<ResourceSet>() : resources;
        }

        public static ICollection<ResourceSet> Resources;

        public Task<bool> Delete(string id)
        {
            lock(_obj)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentNullException(nameof(id));
                }

                var policy = Resources.FirstOrDefault(p => p.Id == id);
                if (policy == null)
                {
                    return Task.FromResult(false);
                }

                Resources.Remove(policy);
                return Task.FromResult(true);
            }
        }

        public Task<ResourceSet> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = Resources.FirstOrDefault(p => p.Id == id);
            if (rec == null)
            {
                return Task.FromResult((ResourceSet)null);
            }

            var result = rec.Copy();
            Enrich(result);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            IEnumerable<ResourceSet> result = Resources.Where(r => ids.Contains(r.Id))
                .Select(r => r.Copy())
                .ToList();
            foreach(var r in result)
            {
                Enrich(r);
            }

            return Task.FromResult(result);
        }

        public Task<ICollection<ResourceSet>> GetAll()
        {
            ICollection<ResourceSet> result = Resources.Select(r => r.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<bool> Insert(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            Resources.Add(resourceSet.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ResourceSet> result = Resources.Select(r => r.Copy()).Select(r => Enrich(r)).ToList();
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names != null && parameter.Names.Any())
            {
                result = result.Where(r => parameter.Names.Any(n => r.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                result = result.Where(r => parameter.Types.Any(t => r.Type.Contains(t)));
            }

            if (parameter.Owners != null && parameter.Owners.Any())
            {
                result = result.Where(r => parameter.Owners.Contains(r.Owner));
            }

            if (parameter.Subjects != null && parameter.Subjects.Any())
            {
                result = result.Where(r => r.AuthPolicies.Any(p => p.Claims != null && p.Claims.Any(c => c.Type == "sub" && parameter.Subjects.Contains(c.Value))));
            }

            var nbResult = result.Count();
            result = result.OrderBy(c => c.Id);
            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            var content = result.ToList();
            return Task.FromResult(new SearchResourceSetResult
            {
                Content = content,
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> Update(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = Resources.FirstOrDefault(p => p.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds;
            rec.IconUri = resourceSet.IconUri;
            rec.Name = resourceSet.Name;
            rec.Scopes = resourceSet.Scopes;
            rec.Type = resourceSet.Type;
            rec.Uri = resourceSet.Uri;
            rec.Owner = resourceSet.Owner;
            rec.Uri = resourceSet.Uri;
            rec.AcceptPendingRequest = resourceSet.AcceptPendingRequest;
            return Task.FromResult(true);
        }

        private static ResourceSet Enrich(ResourceSet resourceSet)
        {
            var policies = DefaultPolicyRepository.Policies.Where(p => p.ResourceSetIds.Contains(resourceSet.Id));
            resourceSet.AuthPolicies = policies.ToList();
            return resourceSet;
        }
    }
}
