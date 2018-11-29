using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class ResourceScopeMapping
    {
        public static ModelBuilder AddResourceScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceScope>()
                .ToTable("ResourceScopes")
                .HasKey(a => new { a.ResourceId, a.Scope });
            return modelBuilder;
        }
    }
}
