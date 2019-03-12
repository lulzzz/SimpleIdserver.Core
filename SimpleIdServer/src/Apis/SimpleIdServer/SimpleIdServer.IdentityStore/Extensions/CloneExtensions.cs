using SimpleIdServer.IdentityStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdServer.IdentityStore.Extensions
{
    internal static class CloneExtensions
    {
        public static User Copy(this User user)
        {
            return new User
            {
                Claims = user.Claims == null ? new List<Claim>() : user.Claims.Select(c => c.Copy()).ToList(),
                CreateDateTime = user.CreateDateTime,
                Id = user.Id,
                IsBlocked = user.IsBlocked,
                UpdateDateTime = user.UpdateDateTime,
                Credentials = user.Credentials == null ? new List<UserCredential>() : user.Credentials.Select(c =>

                    new UserCredential
                    {
                        BlockedDateTime = c.BlockedDateTime,
                        ExpirationDateTime = c.ExpirationDateTime,
                        FirstAuthenticationFailureDateTime = c.FirstAuthenticationFailureDateTime,
                        IsBlocked = c.IsBlocked,
                        NumberOfAttempts = c.NumberOfAttempts,
                        Type = c.Type,
                        Value = c.Value
                    }
                )
            };
        }

        public static Claim Copy(this Claim claim)
        {
            return new Claim(claim.Type, claim.Value);
        }
        }
}
