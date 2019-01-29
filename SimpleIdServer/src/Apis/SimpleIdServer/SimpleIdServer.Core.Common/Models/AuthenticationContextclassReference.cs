using System.Collections.Generic;

namespace SimpleIdServer.Core.Common.Models
{
    public enum AuthenticationContextclassReferenceTypes
    {
        LOA1 = 0,
        LOA2 = 1,
        LOA3 = 2,
        LOA4 = 3
    }

    public class AuthenticationContextclassReference
    {
        /// <summary>
        /// Unique name for examples : sid::loa-1 or sid::loa-2 or sid::loa-3 or sid::loa-4
        /// </summary>
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string DisplayName { get; set; }
        public AuthenticationContextclassReferenceTypes Type { get; set; }
        /// <summary>
        /// List of authentication method references
        /// </summary>
        public IEnumerable<string> AmrLst { get; set; }
    }
}
