namespace SimpleIdServer.Core.Parameters
{
    public class ChangePasswordParameter
    {
        public string NewPassword { get; set; }
        public string ActualPassword { get; set; }
        public string Subject { get; set; }
    }
}
