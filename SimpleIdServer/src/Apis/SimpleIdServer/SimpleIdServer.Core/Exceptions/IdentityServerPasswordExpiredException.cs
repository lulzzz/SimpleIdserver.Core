using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.IdentityStore.Models;

namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerPasswordExpiredException : IdentityServerAuthenticationException
    {
        public IdentityServerPasswordExpiredException(User user) : base(string.Empty)
        {
            User = user;
        }

        public User User{ get; private set; }
    }
}
