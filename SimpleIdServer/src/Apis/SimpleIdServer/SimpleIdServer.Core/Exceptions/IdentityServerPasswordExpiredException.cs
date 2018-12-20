using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerPasswordExpiredException : IdentityServerAuthenticationException
    {
        public IdentityServerPasswordExpiredException(ResourceOwner resourceOwner) : base(string.Empty)
        {
            ResourceOwner = resourceOwner;
        }

        public ResourceOwner ResourceOwner { get; private set; }
    }
}
