using System;
using System.Collections.Generic;

namespace SimpleIdServer.EF.Models
{
    public class ResourceOwner
    {        
        /// <summary>
        /// Get or sets the subject-identifier for the End-User at the issuer.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual ICollection<Consent> Consents { get; set; } 
        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        public virtual ICollection<Profile> Profiles { get; set; }
    }
}
