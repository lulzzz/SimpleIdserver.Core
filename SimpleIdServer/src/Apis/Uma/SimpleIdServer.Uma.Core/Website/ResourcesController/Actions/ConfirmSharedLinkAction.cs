using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController.Actions
{
    public interface IConfirmSharedLinkAction
    {
        Task<bool> Execute(ConfirmSharedLinkParameter confirmSharedLinkParameter);
    }

    internal sealed class ConfirmSharedLinkAction : IConfirmSharedLinkAction
    {
        private readonly ISharedLinkRepository _sharedLinkRepository;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IPolicyRepository _policyRepository;

        public ConfirmSharedLinkAction(ISharedLinkRepository sharedLinkRepository, IResourceSetRepository resourceSetRepository, IPolicyRepository policyRepository)
        {
            _sharedLinkRepository = sharedLinkRepository;
            _resourceSetRepository = resourceSetRepository;
            _policyRepository = policyRepository;
        }

        public async Task<bool> Execute(ConfirmSharedLinkParameter confirmSharedLinkParameter)
        {
            if (confirmSharedLinkParameter == null)
            {
                throw new ArgumentNullException(nameof(confirmSharedLinkParameter));
            }

            if (string.IsNullOrWhiteSpace(confirmSharedLinkParameter.ConfirmationCode))
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheConfirmationCodeMustBeSpecified);
            }

            var sharedLink = await _sharedLinkRepository.Get(confirmSharedLinkParameter.ConfirmationCode).ConfigureAwait(false);
            if (sharedLink == null)
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheConfirmationCodeIsInvalid);
            }

            var resourceId = sharedLink.ResourceId;
            var resource = await _resourceSetRepository.Get(resourceId).ConfigureAwait(false);
            if (resource == null)
            {
                throw new BaseUmaException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
            }

            if (resource.Owner == confirmSharedLinkParameter.Subject)
            {
                throw new BaseUmaException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheSharedLinkCannotBeUsedByTheOwner);
            }

            var policy = new Policy
            {
                Id = Guid.NewGuid().ToString(),
                ResourceSetIds = new List<string> { resourceId },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsResourceOwnerConsentNeeded = false,
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "sub",
                                Value = confirmSharedLinkParameter.Subject
                            }
                        },
                        OpenIdProvider = confirmSharedLinkParameter.OpenidProvider,
                        Scopes = sharedLink.Scopes.ToList()
                    }
                }
            };
            
            using (var transaction = new CommittableTransaction(new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    await _sharedLinkRepository.Delete(sharedLink.ConfirmationCode).ConfigureAwait(false);
                    await _policyRepository.Add(policy).ConfigureAwait(false);
                    transaction.Commit();
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
