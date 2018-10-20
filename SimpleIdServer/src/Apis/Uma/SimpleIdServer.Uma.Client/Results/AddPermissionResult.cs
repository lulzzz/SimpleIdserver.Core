using SimpleIdServer.Common.Client;
using SimpleIdServer.Uma.Common.DTOs;

namespace SimpleIdServer.Uma.Client.Results
{
    public class AddPermissionResult : BaseResponse
    {
        public AddPermissionResponse Content { get; set; }
    }
}
