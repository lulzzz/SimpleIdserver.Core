using SimpleIdServer.Uma.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Repositories
{
    internal sealed class DefaultPendingRequestRepository : IPendingRequestRepository
    {
        private readonly List<PendingRequest> _pendingRequestLst;

        public DefaultPendingRequestRepository()
        {
            _pendingRequestLst = new List<PendingRequest>();
        }

        public Task<bool> Add(PendingRequest pendingRequest)
        {
            _pendingRequestLst.Add(new PendingRequest
            {
                AuthorizationPolicyRuleId = pendingRequest.AuthorizationPolicyRuleId,
                CreateDateTime = DateTime.UtcNow,
                IsConfirmed = pendingRequest.IsConfirmed,
                RequesterSubject = pendingRequest.RequesterSubject
            });
            return Task.FromResult(true);
        }

        public Task<PendingRequest> Get(string policyRuleId, string subject)
        {
            return Task.FromResult(_pendingRequestLst.FirstOrDefault(p => p.RequesterSubject == subject && p.AuthorizationPolicyRuleId == policyRuleId));
        }
    }
}
