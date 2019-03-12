using SimpleIdServer.IdentityStore.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IGetUserOperation
    {
        Task<IdentityStore.Models.User> Execute(string subject);
    }

    internal class GetUserOperation : IGetUserOperation
    {
        private readonly IUserRepository _userRepository;

        public GetUserOperation(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public Task<IdentityStore.Models.User> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }
            
            
            return _userRepository.Get(subject);
        }
    }
}
