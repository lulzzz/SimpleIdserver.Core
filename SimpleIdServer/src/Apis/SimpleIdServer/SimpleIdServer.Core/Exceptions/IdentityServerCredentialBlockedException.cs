namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerCredentialBlockedException : IdentityServerAuthenticationException
    {
        public IdentityServerCredentialBlockedException() : base(string.Empty) { }
    }
}
