using System;
using System.Collections.Generic;

namespace SimpleIdServer.EF.Models
{
    public enum ScopeType
    {
        ProtectedApi,
        ResourceOwner
    }

    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets a boolean whether the scope is an open-id scope.
        /// </summary>
        public bool IsOpenIdScope { get; set; }
        /// <summary>
        /// Gets or sets a boolean whether the scope will be displayed in the consent screen or not.
        /// </summary>
        public bool IsDisplayedInConsent { get; set; }
        /// <summary>
        /// Gets or sets a boolean whether the scope will be displayed in the well-known configuration endpoint.
        /// </summary>
        public bool IsExposed { get; set; }
        public ScopeType Type { get; set; }
        public virtual ICollection<ScopeClaim> ScopeClaims { get; set; }
        public virtual ICollection<ClientScope> ClientScopes { get; set; }
        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual ICollection<ConsentScope> ConsentScopes { get; set; } 
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
