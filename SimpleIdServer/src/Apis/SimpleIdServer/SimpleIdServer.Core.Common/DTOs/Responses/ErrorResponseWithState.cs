using SimpleIdServer.Common.Dtos.Responses;
using System.Runtime.Serialization;

namespace SimpleIdServer.Core.Common.DTOs.Responses
{
    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        [DataMember(Name = ErrorResponseWithStateNames.State)]
        public string State { get; set; }
    }
}
