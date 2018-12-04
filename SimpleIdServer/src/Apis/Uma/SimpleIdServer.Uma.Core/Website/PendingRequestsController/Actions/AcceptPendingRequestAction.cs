using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleIdServer.Uma.Core.Website.PendingRequestsController.Actions
{
    public interface IAcceptPendingRequestAction
    {
        Task<bool> Execute(string subject, string id);
    }

    internal sealed class AcceptPendingRequestAction : IAcceptPendingRequestAction
    {
        private readonly IPendingRequestRepository _pendingRequestRepository;
        private readonly IPolicyRepository _policyRepository;

        public AcceptPendingRequestAction(IPendingRequestRepository pendingRequestRepository, IPolicyRepository policyRepository)
        {
            _pendingRequestRepository = pendingRequestRepository;
            _policyRepository = policyRepository;
        }

        public async Task<bool> Execute(string subject, string id)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _pendingRequestRepository.Get(id).ConfigureAwait(false);
            if (result == null)
            {
                throw new UmaPolicyNotFoundException();
            }

            if (result.Resource.Owner != subject)
            {
                throw new UmaNotAuthorizedException();
            }

            using (var transaction = new CommittableTransaction(new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    result.IsConfirmed = true;
                    await _pendingRequestRepository.Delete(result.Id).ConfigureAwait(false);
                    await _policyRepository.Add(new Models.Policy
                    {
                        Id = Guid.NewGuid().ToString(),
                        Claims = new List<Models.Claim>
                        {
                            new Models.Claim
                            {
                                Type = SimpleIdServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                                Value = result.RequesterSubject
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            result.ResourceId
                        },
                        Scopes = result.Scopes == null ? new List<string>() : result.Scopes.ToList()
                    }).ConfigureAwait(false);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return true;
        }
    }
}
