using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class TranslationMapping
    {
        public static void AddTranslationMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Translation>()
                .ToTable("translations")
                .HasKey(p => new { p.Code, p.LanguageTag });
            modelBuilder.Entity<Translation>()
                .Property(p => p.Code)
                .HasMaxLength(255);
        }
    }
}
