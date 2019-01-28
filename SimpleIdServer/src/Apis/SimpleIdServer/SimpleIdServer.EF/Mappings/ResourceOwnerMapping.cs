using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    public static class ResourceOwnerMapping
    {
        public static void AddResourceOwnerMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwner>()
                .ToTable("resourceOwners")
                .HasKey(j => j.Id);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(r => r.Claims)
                .WithOne(o => o.ResourceOwner)
                .HasForeignKey(a => a.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(r => r.Consents)
                .WithOne(c => c.ResourceOwner)
                .HasForeignKey(a => a.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(r => r.Profiles)
                .WithOne(r => r.ResourceOwner)
                .HasForeignKey(a => a.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(c => c.Credentials)
                .WithOne(s => s.ResourceOwner)
                .HasForeignKey(s => s.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
