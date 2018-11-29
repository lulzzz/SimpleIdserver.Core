namespace SimpleIdServer.Uma.Core.Parameters
{
    public class ConfirmSharedLinkParameter
    {
        public string Subject { get; set; }
        public string ConfirmationCode { get; set; }
        public string OpenidProvider { get; set; }
    }
}