using System.Collections.Generic;

namespace SimpleIdServer.Authenticate.Basic
{
    public class BasicAuthenticateOptions
    {
        public BasicAuthenticateOptions()
        {
            ClaimsIncludedInUserCreation = new List<string>();
            IsEditCredentialEnabled = false;
        }

        /// <summary>
        /// List of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public IEnumerable<string> ClaimsIncludedInUserCreation { get; set; }
        /// <summary>
        /// Can edit the password
        /// </summary>
        public bool IsEditCredentialEnabled { get; set; }
    }
}
