using SimpleIdServer.Common.Dtos.Responses;
using System.Runtime.Serialization;

namespace SimpleIdServer.Dtos.Responses
{
    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        [DataMember(Name = Constants.ErrorResponseWithStateNames.State)]
        public string State { get; set; }
    }
}
