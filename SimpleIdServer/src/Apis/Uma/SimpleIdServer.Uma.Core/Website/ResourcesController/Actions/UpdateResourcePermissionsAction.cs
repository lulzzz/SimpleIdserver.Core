using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController.Actions
{
    public interface IUpdateResourcePermissionsAction
    {
        Task<bool> Execute(UpdateResourcePermissionsParameter updateResourcePermissionsParameter);
    }

    internal sealed class UpdateResourcePermissionsAction : IUpdateResourcePermissionsAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IPolicyRepository _policyRepository;

        public UpdateResourcePermissionsAction(IResourceSetRepository resourceSetRepository, IPolicyRepository policyRepository)
        {
            _resourceSetRepository = resourceSetRepository;
            _policyRepository = policyRepository;
        }

        public async Task<bool> Execute(UpdateResourcePermissionsParameter updateResourcePermissionsParameter)
        {
            if (updateResourcePermissionsParameter == null)
            {
                throw new ArgumentNullException(nameof(updateResourcePermissionsParameter));
            }

            if (string.IsNullOrWhiteSpace(updateResourcePermissionsParameter.ResourceId))
            {
                throw new BaseUmaException(Errors.ErrorCodes.InvalidRequestCode, Errors.ErrorDescriptions.TheResourceIdMustBeSpecified);
            }

            var resource = await _resourceSetRepository.Get(updateResourcePermissionsParameter.ResourceId).ConfigureAwait(false);
            if (resource == null)
            {
                throw new UmaResourceNotFoundException();
            }

            if (updateResourcePermissionsParameter.Subject != resource.Owner)
            {
                throw new UmaNotAuthorizedException();
            }

            var policiesToBeUpdated = resource.Policies.ToList();
            var policiesToBeRemoved = new List<string>();
            var length = policiesToBeUpdated.Count();
            for(int i = length - 1; i >= 0; i--)
            {
                var policy = policiesToBeUpdated.ElementAt(i);
                var policyParameter = updateResourcePermissionsParameter.Policies.FirstOrDefault(p => p.PolicyId == policy.Id);
                if (policyParameter == null)
                {
                    policiesToBeUpdated.Remove(policy);
                    policiesToBeRemoved.Add(policy.Id);
                }
                else
                {
                    var rulesLength = policy.Rules.Count();
                    for (int s = rulesLength - 1; s >= 0; s--)
                    {
                        var rule = policy.Rules[s];
                        if (!policyParameter.RuleIds.Contains(rule.Id))
                        {
                            policy.Rules.Remove(rule);
                        }
                    }
                }
            }

            using (var transaction = new CommittableTransaction(new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    var operations = new List<Task<bool>>();
                    foreach(var policy in policiesToBeUpdated)
                    {
                        operations.Add(_policyRepository.Update(policy));
                    }

                    foreach(var policyId in policiesToBeRemoved)
                    {
                        operations.Add(_policyRepository.Delete(policyId));
                    }

                    await Task.WhenAll(operations).ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
