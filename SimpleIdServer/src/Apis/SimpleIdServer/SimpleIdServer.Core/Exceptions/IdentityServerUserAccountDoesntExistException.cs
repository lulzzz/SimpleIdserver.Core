namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerUserAccountDoesntExistException : IdentityServerAuthenticationException
    {
        public IdentityServerUserAccountDoesntExistException() : base(string.Empty)
        {
        }
    }
}
