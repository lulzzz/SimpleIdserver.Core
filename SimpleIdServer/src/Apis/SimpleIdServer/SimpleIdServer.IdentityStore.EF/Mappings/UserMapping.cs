using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF.Mappings
{
    internal static class UserMapping
    {
        public static ModelBuilder AddUserMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.User>()
                .ToTable("users")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Models.User>()
                .HasMany(r => r.Claims)
                .WithOne(o => o.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.User>()
                .HasMany(c => c.Credentials)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
