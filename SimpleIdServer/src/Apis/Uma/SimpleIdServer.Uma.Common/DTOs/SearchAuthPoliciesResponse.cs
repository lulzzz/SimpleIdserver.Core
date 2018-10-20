using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SimpleIdServer.Uma.Common.DTOs
{
    [DataContract]
    public class SearchAuthPoliciesResponse
    {
        [JsonProperty(SearchResponseNames.Content)]
        [DataMember(Name = SearchResponseNames.Content)]
        public IEnumerable<PolicyResponse> Content { get; set; }

        [JsonProperty(SearchResponseNames.TotalResults)]
        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [JsonProperty(SearchResponseNames.StartIndex)]
        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
