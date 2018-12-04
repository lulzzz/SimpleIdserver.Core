using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    internal sealed class DefaultPendingRequestRepository : IPendingRequestRepository
    {
        private readonly List<PendingRequest> _pendingRequestLst;
        private static object _obj = new object();

        public DefaultPendingRequestRepository()
        {
            _pendingRequestLst = new List<PendingRequest>();
        }

        public Task<bool> Add(PendingRequest pendingRequest)
        {
            _pendingRequestLst.Add(new PendingRequest
            {
                Id = pendingRequest.Id,
                Scopes = pendingRequest.Scopes,
                ResourceId = pendingRequest.ResourceId,
                CreateDateTime = DateTime.UtcNow,
                IsConfirmed = pendingRequest.IsConfirmed,
                RequesterSubject = pendingRequest.RequesterSubject
            });
            return Task.FromResult(true);
        }

        public Task<PendingRequest> Get(string id)
        {
            var result = _pendingRequestLst.FirstOrDefault(p => p.Id == id);
            if (result == null)
            {
                return Task.FromResult((PendingRequest)null);
            }

            Enrich(result);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<PendingRequest>> Get(string resourceId, string subject)
        {
            var result = _pendingRequestLst.Where(p => p.RequesterSubject == subject && p.ResourceId == resourceId);
            foreach(var r in result)
            {
                Enrich(r);
            }

            return Task.FromResult(result);
        }

        public Task<bool> Update(PendingRequest pendingRequest)
        {
            var record = _pendingRequestLst.FirstOrDefault(p => p.Id == pendingRequest.Id);
            if (record == null)
            {
                return Task.FromResult(false);
            }

            record.IsConfirmed = pendingRequest.IsConfirmed;
            return Task.FromResult(true);
        }

        public Task<SearchPendingRequestResult> Search(SearchPendingRequestParameter parameter)
        {
            var result = new SearchPendingRequestResult();
            var content = _pendingRequestLst.Select(p =>
            {
                var r = new PendingRequest
                {
                    Id = p.Id,
                    Scopes = p.Scopes,                    
                    ResourceId = p.ResourceId,
                    CreateDateTime = p.CreateDateTime,
                    IsConfirmed = p.IsConfirmed,
                    RequesterSubject = p.RequesterSubject
                };
                Enrich(r);
                return r;
            });

            if(parameter.Owners != null)
            {
                content = content.Where(c => parameter.Owners.Contains(c.Resource.Owner));
            }

            if (parameter.IsConfirmed != null)
            {
                content = content.Where(c => c.IsConfirmed == parameter.IsConfirmed.Value);
            }

            var count = _pendingRequestLst.Count();
            if (parameter.IsPagingEnabled)
            {
                content = content.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            result.PendingRequests = content;
            result.TotalResults = count;
            result.StartIndex = parameter.StartIndex;
            return Task.FromResult(result);
        }

        public Task<bool> Delete(string id)
        {
            lock(_obj)
            {
                var record = _pendingRequestLst.FirstOrDefault(p => p.Id == id);
                if (record == null)
                {
                    return Task.FromResult(false);
                }

                _pendingRequestLst.Remove(record);
                return Task.FromResult(true);
            }
        }

        private static void Enrich(PendingRequest request)
        {
            if (request == null)
            {
                return;
            }

            request.Resource = DefaultResourceSetRepository.Resources.FirstOrDefault(r => r.Id == request.ResourceId);
        }
    }
}
