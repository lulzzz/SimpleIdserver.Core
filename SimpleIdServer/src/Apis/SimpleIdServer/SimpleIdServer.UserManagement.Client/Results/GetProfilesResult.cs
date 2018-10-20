using System.Collections.Generic;
using SimpleIdServer.Common.Client;
using SimpleIdServer.UserManagement.Common.Responses;

namespace SimpleIdServer.UserManagement.Client.Results
{
    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}