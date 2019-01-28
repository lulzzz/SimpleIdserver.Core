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
        /// Gets or sets the two factor authentication
        /// </summary>
        public string TwoFactorAuthentication { get; set; }
        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        public virtual ICollection<ResourceOwnerClaim> Claims { get; set; }
        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual ICollection<Consent> Consents { get; set; } 
        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        public virtual ICollection<Profile> Profiles { get; set; }
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public virtual ICollection<ResourceOwnerCredential> Credentials { get; set; }
        /// <summary>
        /// Gets or sets the create datetime.
        /// </summary>
        public DateTime CreateDateTime { get; set; }
        /// <summary>
        /// Gets or sets the update datetime.
        /// </summary>
        public DateTime UpdateDateTime { get; set; }
        /// <summary>
        /// Gets or sets is blocked.
        /// </summary>
        public bool IsBlocked { get; set; }
    }
}
