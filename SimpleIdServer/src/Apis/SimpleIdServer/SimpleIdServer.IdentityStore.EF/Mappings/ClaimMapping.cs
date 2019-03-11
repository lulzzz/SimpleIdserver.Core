using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF.Mappings
{
    internal static class ClaimMapping
    {
        public static ModelBuilder AddClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Claim>()
                .ToTable("claims")
                .HasKey(c => c.Code);
            modelBuilder.Entity<Models.Claim>()
                .HasMany(c => c.UserClaims)
                .WithOne(s => s.Claim)
                .HasForeignKey(s => s.ClaimCode)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
