﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the password expiration datetime.
        /// </summary>
        public DateTime PasswordExpirationDateTime { get; set; }
        /// <summary>
        /// Gets or sets the blocked datetime.
        /// </summary>
        public DateTime BlockedDateTime { get; set; }
        /// <summary>
        /// Gets or sets the two factor authentication
        /// </summary>
        public string TwoFactorAuthentication { get; set; }
        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        public virtual List<ResourceOwnerClaim> Claims { get; set; }
        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual List<Consent> Consents { get; set; } 
        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        public virtual ICollection<Profile> Profiles { get; set; }
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
        /// <summary>
        /// Gets or sets the number of attemps.
        /// </summary>
        public int NumberOfAttempts { get; set; }
        /// <summary>
        /// Gets or sets the first authentication failure datetime.
        /// </summary>
        public DateTime? FirstAuthenticationFailureDateTime { get; set; }
    }
}
