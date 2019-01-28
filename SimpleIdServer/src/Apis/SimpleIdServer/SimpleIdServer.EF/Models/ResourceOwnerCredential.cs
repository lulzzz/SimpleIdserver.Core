using System;

namespace SimpleIdServer.EF.Models
{
    public class ResourceOwnerCredential
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the resource owner identifier.
        /// </summary>
        public string ResourceOwnerId { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Gets or sets the first authentication datetime.
        /// </summary>
        public DateTime? FirstAuthenticationFailureDateTime { get; set; }
        /// <summary>
        /// Gets or sets the number of authentication attempts.
        /// </summary>
        public int NumberOfAttempts { get; set; }
        /// <summary>
        /// Gets or sets the blocked datetime.
        /// </summary>
        public DateTime BlockedDateTime { get; set; }
        /// <summary>
        /// Gets or sets the blocked.
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Gets or sets the expiration datetime.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
        /// <summary>
        /// Gets or sets the resource owner.
        /// </summary>
        public virtual ResourceOwner ResourceOwner { get; set; }
    }
}
