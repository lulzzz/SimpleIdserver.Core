using System.Net;
using SimpleIdServer.Common.Dtos.Responses;

namespace SimpleIdServer.Common.Client
{
    public class BaseResponse
    {
        public HttpStatusCode HttpStatus { get; set; }
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}