using System;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;

namespace SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions
{
    public interface ISearchResourceSetOperation
    {
        Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter);
    }

    internal sealed class SearchResourceSetOperation : ISearchResourceSetOperation
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public SearchResourceSetOperation(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _resourceSetRepository.Search(parameter);
        }
    }
}
