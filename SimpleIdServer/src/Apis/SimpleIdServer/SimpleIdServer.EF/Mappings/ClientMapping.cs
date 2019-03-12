using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ClientMapping
    {
        public static void AddClientMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Client>()
                .ToTable("clients")
                .HasKey(c => c.ClientId);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.ClientScopes)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.JsonWebKeys)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.Consents)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.ClientSecrets)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
