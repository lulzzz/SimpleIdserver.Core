using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Common.Dtos.Responses
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = Constants.ErrorResponseNames.Error)]
        public string Error { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.ErrorDescription)]
        public string ErrorDescription { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.ErrorUri)]
        public string ErrorUri { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.ErrorDetails)]
        public IEnumerable<object> ErrorDetails { get; set; }
    }
}
