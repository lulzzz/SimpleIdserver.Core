using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public sealed class DatatableRequest
    {
        [DataMember(Name = "draw")]
        public int Draw { get; set; }
        [DataMember(Name = "start")]
        public int Start { get; set; }
        [DataMember(Name = "length")]
        public int Length { get; set; }
        [DataMember(Name = "columns")]
        public IEnumerable<DatatableColumnRequest> Columns { get; set; }
    }
}
