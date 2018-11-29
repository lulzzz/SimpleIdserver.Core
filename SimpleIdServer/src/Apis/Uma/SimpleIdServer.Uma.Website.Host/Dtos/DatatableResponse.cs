using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Website.Host.Dtos
{
    [DataContract]
    public class DatatableResponse
    {
        [DataMember(Name = "draw")]
        public int Draw { get; set; }
        [DataMember(Name = "recordsTotal")]
        public int RecordsTotal { get; set; }
        [DataMember(Name = "recordsFiltered")]
        public int RecordsFiltered { get; set; }
        [DataMember(Name = "data")]
        public IEnumerable<IEnumerable<object>> Data { get; set; }
    }
}
