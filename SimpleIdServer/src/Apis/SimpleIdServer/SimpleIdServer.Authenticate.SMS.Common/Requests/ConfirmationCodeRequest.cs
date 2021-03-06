﻿using System.Runtime.Serialization;

namespace SimpleIdServer.Authenticate.SMS.Common.Requests
{
    [DataContract]
    public class ConfirmationCodeRequest
    {
        [DataMember(Name = Constants.ConfirmationCodeRequestNames.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}