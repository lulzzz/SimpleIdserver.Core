using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ResourceOwnerCredentialMapping
    {
        public static ModelBuilder AddResourceCredentialMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwnerCredential>()
                .ToTable("resourceOwnerCredentials")
                .HasKey(p => new { p.ResourceOwnerId, p.Type });
            return modelBuilder;
        }
    }
}