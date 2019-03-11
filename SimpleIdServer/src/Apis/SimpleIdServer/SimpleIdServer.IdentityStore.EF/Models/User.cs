using System;
using System.Collections.Generic;

namespace SimpleIdServer.IdentityStore.EF.Models
{
    public class User
    {
        /// <summary>
        /// Get or sets the subject-identifier for the End-User at the issuer.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        public virtual ICollection<UserClaim> Claims { get; set; }
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public virtual ICollection<UserCredential> Credentials { get; set; }
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
