using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.EF.Extensions;
using SimpleIdServer.Uma.EF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.EF.Repositories
{
    internal sealed class PendingRequestRepository : IPendingRequestRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public PendingRequestRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<PendingRequest> Get(string id)
        {
            var record = await _context.PendingRequests.FirstOrDefaultAsync(p => p.Id == id);
            if (record == null)
            {
                return null;
            }

            return record.ToDomain();
        }

        public async Task<bool> Add(PendingRequest pendingRequest)
        {
            _context.PendingRequests.Add(new Models.ResourcePendingRequest
            {
                Id = pendingRequest.Id,
                Scopes = pendingRequest.Scopes == null ? new List<PendingRequestScope>() : pendingRequest.Scopes.Select(s => new PendingRequestScope
                    {
                        PendingRequestId = pendingRequest.Id,
                        Scope = s
                    }
                ).ToList(),
                CreateDateTime = pendingRequest.CreateDateTime,
                IsConfirmed = pendingRequest.IsConfirmed,
                ResourceId = pendingRequest.ResourceId,
                RequesterSubject = pendingRequest.RequesterSubject
            });
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<IEnumerable<PendingRequest>> Get(string resourceId, string subject)
        {
            var pendingRequests = _context.PendingRequests
                .Include(p => p.Resource)
                .Include(p => p.Scopes);
            var result = await pendingRequests.Where(c => c.RequesterSubject == subject && c.ResourceId == resourceId).ToListAsync().ConfigureAwait(false);
            return result.Select(r => r.ToDomain());
        }

        public async Task<bool> Update(PendingRequest pendingRequest)
        {
            var result = await _context.PendingRequests.FirstOrDefaultAsync(c => c.Id == pendingRequest.Id).ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            result.IsConfirmed = pendingRequest.IsConfirmed;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<SearchPendingRequestResult> Search(SearchPendingRequestParameter parameter)
        {
            var result = new SearchPendingRequestResult();
            IQueryable<ResourcePendingRequest> content = _context.PendingRequests.Include(p => p.Resource).Include(p => p.Scopes);
            if (parameter.Owners != null)
            {
                content = content.Where(c => parameter.Owners.Contains(c.Resource.Owner));
            }

            if (parameter.IsConfirmed != null)
            {
                content = content.Where(c => c.IsConfirmed == parameter.IsConfirmed.Value);
            }

            var totalResults = await _context.PendingRequests.CountAsync().ConfigureAwait(false);
            if (parameter.IsPagingEnabled)
            {
                content = content.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            var lst = await content.ToListAsync().ConfigureAwait(false);
            result.TotalResults = totalResults;
            result.PendingRequests = lst.Select(r => r.ToDomain());
            result.StartIndex = parameter.StartIndex;
            return result;
        }

        public async Task<bool> Delete(string id)
        {
            var record = await _context.PendingRequests.FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.PendingRequests.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
