using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Domain = SimpleIdServer.IdentityStore.Models;
using Model = SimpleIdServer.IdentityStore.EF.Models;

namespace SimpleIdServer.IdentityStore.EF.Extensions
{
    internal static class MappingExtensions
    {
        public static Domain.CredentialSetting ToDomain(this Model.CredentialSetting credentialSetting)
        {
            if (credentialSetting == null)
            {
                throw new ArgumentNullException(nameof(credentialSetting));
            }

            return new Domain.CredentialSetting
            {
                CredentialType = credentialSetting.CredentialType,
                ExpiresIn = credentialSetting.ExpiresIn,
                Options = credentialSetting.Options,
                AuthenticationIntervalsInSeconds = credentialSetting.AuthenticationIntervalsInSeconds,
                IsBlockAccountPolicyEnabled = credentialSetting.IsBlockAccountPolicyEnabled,
                NumberOfAuthenticationAttempts = credentialSetting.NumberOfAuthenticationAttempts
            };
        }

        public static Domain.User ToDomain(this Model.User resourceOwner)
        {
            if (resourceOwner == null)
            {
                return null;
            }

            var claims = new List<Claim>();
            var credentials = new List<Domain.UserCredential>();
            if (resourceOwner.Claims != null && resourceOwner.Claims.Any())
            {
                foreach (var c in resourceOwner.Claims)
                {
                    claims.Add(new Claim(c.ClaimCode, c.Value));
                }
            }

            if (resourceOwner.Credentials != null && resourceOwner.Credentials.Any())
            {
                foreach (var cr in resourceOwner.Credentials)
                {
                    credentials.Add(new Domain.UserCredential
                    {
                        BlockedDateTime = cr.BlockedDateTime,
                        ExpirationDateTime = cr.ExpirationDateTime,
                        FirstAuthenticationFailureDateTime = cr.FirstAuthenticationFailureDateTime,
                        IsBlocked = cr.IsBlocked,
                        NumberOfAttempts = cr.NumberOfAttempts,
                        Type = cr.Type,
                        Value = cr.Value
                    });
                }
            }

            return new Domain.User
            {
                Id = resourceOwner.Id,
                IsBlocked = resourceOwner.IsBlocked,
                Claims = claims,
                CreateDateTime = resourceOwner.CreateDateTime,
                UpdateDateTime = resourceOwner.UpdateDateTime,
                Credentials = credentials
            };
        }
    }
}
