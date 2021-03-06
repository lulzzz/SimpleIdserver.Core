﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Scim.Common.DTOs
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = Constants.ScimResourceNames.Schemas)]
        public IEnumerable<string> Schemas { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.Detail)]
        public string Detail { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.Status)]
        public int Status { get; set; }
    }

    [DataContract]
    public class EnrichedErrorResponse : ErrorResponse
    {
        [DataMember(Name = Constants.EnrichedErrorResponseNames.ScimType)]
        public string ScimType { get; set; }
    }
}
