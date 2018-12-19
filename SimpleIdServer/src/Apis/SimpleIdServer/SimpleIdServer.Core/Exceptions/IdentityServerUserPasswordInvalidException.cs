namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerUserPasswordInvalidException : IdentityServerAuthenticationException
    {
        public IdentityServerUserPasswordInvalidException() : base(string.Empty)
        {
        }
    }
}
