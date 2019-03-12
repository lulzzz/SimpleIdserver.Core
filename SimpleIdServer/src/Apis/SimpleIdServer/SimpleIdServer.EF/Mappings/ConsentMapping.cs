using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ConsentMapping
    {
        public static void AddConsentMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consent>()
                .ToTable("consents")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Consent>()
                .HasMany(r => r.ConsentScopes)
                .WithOne(a => a.Consent)
                .HasForeignKey(fk => fk.ConsentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Consent>()
                .HasMany(r => r.ConsentClaims)
                .WithOne(a => a.Consent)
                .HasForeignKey(fk => fk.ConsentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
