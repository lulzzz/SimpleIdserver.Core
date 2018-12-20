using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;
using System;

namespace SimpleIdServer.EF.Mappings
{
    internal static class PasswordSettingsMapping
    {
        public static ModelBuilder AddPasswordSettingsMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<PasswordSettings>()
                .ToTable("passwordSettings")
                .HasKey(c => c.Id);
            return modelBuilder;
        }
    }
}
