using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class DefaultSettingsMapping
    {
        public static ModelBuilder AddDefaultSettingsMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultSettings>()
                .ToTable("defaultSettings")
                .HasKey(d => d.Id);
            return modelBuilder;
        }
    }
}
