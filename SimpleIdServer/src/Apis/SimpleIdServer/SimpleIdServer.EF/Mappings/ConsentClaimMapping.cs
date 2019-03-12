using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ConsentClaimMapping
    {
        public static void AddConsentClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsentClaim>()
                .ToTable("consentClaims")
                .HasKey(c => new { c.ConsentId, c.ClaimCode });
        }
    }
}
