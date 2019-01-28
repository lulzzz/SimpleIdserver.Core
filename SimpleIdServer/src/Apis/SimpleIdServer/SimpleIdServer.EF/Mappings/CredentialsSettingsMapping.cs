using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;
using System;

namespace SimpleIdServer.EF.Mappings
{
    internal static class PasswordSettingsMapping
    {
        public static ModelBuilder AddCredentialsSettingsMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<CredentialSetting>()
                .ToTable("credentialSettings")
                .HasKey(c => c.CredentialType);
            return modelBuilder;
        }
    }
}
