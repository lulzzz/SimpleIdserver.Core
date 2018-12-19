namespace SimpleIdServer.Core.Exceptions
{
    public class IdentityServerUserTooManyRetryException : IdentityServerAuthenticationException
    {
        public IdentityServerUserTooManyRetryException() : base(string.Empty)
        {

        }

        public int RetryInSeconds { get; set; }
    }
}
