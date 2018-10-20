using System.Net;
using SimpleIdServer.Core.Common.DTOs.Responses;

namespace SimpleIdServer.Client.Results
{
    public class BaseSidResult
    {
        public bool ContainsError { get; set; }
        public ErrorResponseWithState Error { get; set;}
        public HttpStatusCode Status { get; set; }
    }
}
