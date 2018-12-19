namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerUserAccountBlockedException : IdentityServerAuthenticationException
    {
        public IdentityServerUserAccountBlockedException() : base(string.Empty)
        {
        }
    }
}