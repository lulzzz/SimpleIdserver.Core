namespace SimpleIdServer.Authenticate.LoginPassword.Parameters
{
    public sealed class ChangePasswordParameter
    {
        public string ActualPassword { get; set; }
        public string NewPassword { get; set; }
        public string Subject { get; set; }
    }
}
