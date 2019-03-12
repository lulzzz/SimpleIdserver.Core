using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ConsentScopeMapping
    {
        public static void AddConsentScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsentScope>()
                .ToTable("consentScopes")
                .HasKey(c => new { c.ConsentId, c.ScopeName });
        }
    }
}
