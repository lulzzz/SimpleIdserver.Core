using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController.Actions
{
    public interface IGetCurrentUserResourcesAction
    {
        Task<SearchResourceSetResult> Execute(SearchCurrentUserResourceSetParameter searchCurrentUserResourceSetParameter);
    }

    internal sealed class GetCurrentUserResourcesAction : IGetCurrentUserResourcesAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public GetCurrentUserResourcesAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<SearchResourceSetResult> Execute(SearchCurrentUserResourceSetParameter searchCurrentUserResourceSetParameter)
        {
            if (searchCurrentUserResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(searchCurrentUserResourceSetParameter));
            }

            var parameter = new SearchResourceSetParameter
            {
                Owners = new[] { searchCurrentUserResourceSetParameter.Owner },
                StartIndex = searchCurrentUserResourceSetParameter.StartIndex,
                Count = searchCurrentUserResourceSetParameter.Count
            };
            return _resourceSetRepository.Search(parameter);
        }
    }
}
