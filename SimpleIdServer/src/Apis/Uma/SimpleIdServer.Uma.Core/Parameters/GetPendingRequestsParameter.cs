namespace SimpleIdServer.Uma.Core.Parameters
{
    public class GetPendingRequestsParameter
    {
        public string Subject { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
