using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserOperation
    {
        Task<ResourceOwner> Execute(string subject);
    }

    internal class GetUserOperation : IGetUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetUserOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }
        
        public Task<ResourceOwner> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }
            
            
            return _resourceOwnerRepository.GetAsync(subject);
        }
    }
}
