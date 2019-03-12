using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ClaimMapping
    {
        public static void AddClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>()
                .ToTable("claims")
                .HasKey(p => p.Code);
            modelBuilder.Entity<Claim>()
                .HasMany(c => c.ScopeClaims)
                .WithOne(s => s.Claim)
                .HasForeignKey(s => s.ClaimCode)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Claim>()
                .HasMany(c => c.ConsentClaims)
                .WithOne(s => s.Claim)
                .HasForeignKey(s => s.ClaimCode)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
