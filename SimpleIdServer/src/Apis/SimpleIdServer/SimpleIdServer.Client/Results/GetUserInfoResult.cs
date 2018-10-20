﻿using Newtonsoft.Json.Linq;

namespace SimpleIdServer.Client.Results
{
    public class GetUserInfoResult : BaseSidResult
    {
        public JObject Content { get; set; }
        public string JwtToken { get; set; }
    }
}
