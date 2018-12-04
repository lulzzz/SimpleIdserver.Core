using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdServer.Uma.Common.DTOs
{
    [DataContract]
    public class PostResourceSet
    {
        [DataMember(Name = ResourceSetResponseNames.Name)]
        public string Name { get; set; }       
        [DataMember(Name = ResourceSetResponseNames.Uri)]
        public string Uri { get; set; }            
        [DataMember(Name = ResourceSetResponseNames.Type)]
        public string Type { get; set; }          
        [DataMember(Name = ResourceSetResponseNames.IconUri)]
        public string IconUri { get; set; }
        [DataMember(Name = ResourceSetResponseNames.Scopes)]
        public List<string> Scopes { get; set; }
        [DataMember(Name = ResourceSetResponseNames.Owner)]
        public string Owner { get; set; }
        [DataMember(Name = ResourceSetResponseNames.AcceptPendingRequest)]
        public bool AcceptPendingRequest { get; set; }
    }
}