using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ScopeClaimMapping
    {
        public static void AddScopeClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScopeClaim>()
                .ToTable("scopeClaims")
                .HasKey(s => new { s.ClaimCode, s.ScopeName });
        }
    }
}
