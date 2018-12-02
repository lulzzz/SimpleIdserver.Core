using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class ResourceSetPolicyMapping
    {
        public static void AddResourceSetPolicyMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceSetPolicy>()
                .ToTable("ResourceSetPolicies")
                .HasKey(p => new
                {
                    p.PolicyId,
                    p.ResourceSetId
                });
        }
    }
}
