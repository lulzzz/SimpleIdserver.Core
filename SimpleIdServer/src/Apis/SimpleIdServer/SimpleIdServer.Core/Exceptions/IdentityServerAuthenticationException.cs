using System;

namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerAuthenticationException : InternalIdentityServerException
    {
        public IdentityServerAuthenticationException(string message) : base(message)
        {
        }

        public IdentityServerAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}