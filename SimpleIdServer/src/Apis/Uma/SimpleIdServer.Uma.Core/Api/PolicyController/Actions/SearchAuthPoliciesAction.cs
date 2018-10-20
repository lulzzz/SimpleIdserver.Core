using System;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;

namespace SimpleIdServer.Uma.Core.Api.PolicyController.Actions
{
    public interface ISearchAuthPoliciesAction
    {
        Task<SearchAuthPoliciesResult> Execute(SearchAuthPoliciesParameter parameter);
    }

    internal sealed class SearchAuthPoliciesAction : ISearchAuthPoliciesAction
    {
        private readonly IPolicyRepository _policyRepository;

        public SearchAuthPoliciesAction(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public Task<SearchAuthPoliciesResult> Execute(SearchAuthPoliciesParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _policyRepository.Search(parameter);
        }
    }
}
