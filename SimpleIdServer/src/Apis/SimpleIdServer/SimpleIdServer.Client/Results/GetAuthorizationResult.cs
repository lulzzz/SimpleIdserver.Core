using System;

namespace SimpleIdServer.Client.Results
{
    public class GetAuthorizationResult : BaseSidResult
    {
        public Uri Location { get; set; }
    }
}
