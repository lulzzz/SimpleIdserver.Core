using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF.Mappings
{
    internal static class UserClaimMapping
    {
        public static ModelBuilder AddUserClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.User>()
                .ToTable("userClaims")
                .HasKey(c => c.Id);
            return modelBuilder;
        }
    }
}
