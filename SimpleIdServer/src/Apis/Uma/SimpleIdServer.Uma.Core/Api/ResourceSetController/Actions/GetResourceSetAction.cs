using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IGetResourceSetAction
    {
        Task<ResourceSet> Execute(string id);
    }

    internal class GetResourceSetAction : IGetResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public GetResourceSetAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<ResourceSet> Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return _resourceSetRepository.Get(id);
        }
    }
}
