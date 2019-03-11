using System.Threading.Tasks;

namespace SimpleIdServer.IdentityStore.EF.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly IdentityStoreEFContext _context;

        public UserRepository(IdentityStoreEFContext context)
        {
            _context = context;
        }

        public Task<User> Get(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
