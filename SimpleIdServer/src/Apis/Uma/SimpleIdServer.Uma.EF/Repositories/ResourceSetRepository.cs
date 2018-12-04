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
    internal class ResourceSetRepository : IResourceSetRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public ResourceSetRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }
        
        public async Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Models.ResourceSet> resourceSet = _context.ResourceSets
                .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Claims)
                .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Scopes)
                .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Clients)
                .Include(r => r.Scopes);
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                resourceSet = resourceSet.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names != null && parameter.Names.Any())
            {
                resourceSet = resourceSet.Where(r => parameter.Names.Any(n => r.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                resourceSet = resourceSet.Where(r => parameter.Types.Any(t => r.Type.Contains(t)));
            }

            if (parameter.Owners != null && parameter.Owners.Any())
            {
                resourceSet = resourceSet.Where(r => parameter.Owners.Contains(r.Owner));
            }

            if (parameter.Subjects != null && parameter.Subjects.Any())
            {
                resourceSet = resourceSet.Where(r => r.ResourceSetPolicies.Any(p => p.Policy != null &&  p.Policy.Claims != null && p.Policy.Claims.Any(c => c.Key == "sub" && parameter.Subjects.Contains(c.Value))));
            }

            var nbResult = await resourceSet.CountAsync().ConfigureAwait(false);
            resourceSet = resourceSet.OrderBy(c => c.Id);
            if (parameter.IsPagingEnabled)
            {
                resourceSet = resourceSet.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return new SearchResourceSetResult
            {
                Content = await resourceSet.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            };
        }

        public async Task<bool> Insert(ResourceSet resourceSet)
        {
            _context.ResourceSets.Add(resourceSet.ToModel());
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<ResourceSet> Get(string id)
        {
            try
            {
                var resourceSet = await _context.ResourceSets
                    .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Scopes)
                    .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Claims)
                    .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Clients)
                    .Include(r => r.Scopes)
                    .FirstOrDefaultAsync(r => r.Id == id)
                    .ConfigureAwait(false);
                return resourceSet == null ? null : resourceSet.ToDomain();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> Update(ResourceSet resourceSet)
        {
            var record = await _context.ResourceSets
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Scopes)
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Claims)
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Clients)
                 .Include(r => r.Scopes)
                 .FirstOrDefaultAsync(r => r.Id == resourceSet.Id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }
            
            var recordScopes = record.Scopes.Select(s => s.Scope);
            var scopesNotToBeDeleted = new List<string>();
            if (resourceSet.Scopes != null)
            {
                foreach (var scope in resourceSet.Scopes)
                {
                    var s = record.Scopes.FirstOrDefault(c => c.Scope == scope);
                    if (s == null)
                    {
                        s = new Models.ResourceScope
                        {
                            ResourceId = record.Id,
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

            record.Name = resourceSet.Name;
            record.Type = resourceSet.Type;
            record.Uri = resourceSet.Uri;
            record.IconUri = resourceSet.IconUri;
            record.Owner = resourceSet.Owner;
            record.AcceptPendingRequest = resourceSet.AcceptPendingRequest;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<ICollection<ResourceSet>> GetAll()
        {
            return await _context.ResourceSets.Select(r => r.ToDomain()).ToListAsync().ConfigureAwait(false);   
        }

        public async Task<bool> Delete(string id)
        {
            var record = await _context.ResourceSets
                .FirstOrDefaultAsync(r => r.Id == id)
                .ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.ResourceSets.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids)
        {
             return await _context.ResourceSets
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Scopes)
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Claims)
                 .Include(r => r.ResourceSetPolicies).ThenInclude(r => r.Policy).ThenInclude(r => r.Clients)
                 .Include(r => r.Scopes)
                 .Where(r => ids.Contains(r.Id))
                 .Select(r => r.ToDomain())
                 .ToListAsync()
                 .ConfigureAwait(false);
        }
    }
}