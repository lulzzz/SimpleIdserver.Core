using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Website.ResourcesController.Actions
{
    public interface IGetResourcesSharedWithMeAction
    {
        Task<SearchResourceSetResult> Execute(SearchSharedResourcesParameter parameter);
    }

    internal sealed class GetResourcesSharedWithMeAction : IGetResourcesSharedWithMeAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public GetResourcesSharedWithMeAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<SearchResourceSetResult> Execute(SearchSharedResourcesParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }


            return _resourceSetRepository.Search(new SearchResourceSetParameter
            {
                Subjects = new List<string>
                {
                    parameter.Subject
                },
                Count = parameter.Count,
                StartIndex = parameter.StartIndex
            });
        }
    }
}
