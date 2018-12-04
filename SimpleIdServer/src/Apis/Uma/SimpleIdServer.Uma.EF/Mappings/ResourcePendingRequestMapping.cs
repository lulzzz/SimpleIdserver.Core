using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;
using System;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class ResourcePendingRequestMapping
    {
        public static ModelBuilder AddResourcePendingRequestMapping(this ModelBuilder modelBuilder)
        {
            if(modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<ResourcePendingRequest>().ToTable("resourcePendingRequests")
                .HasKey(s => s.Id);
            modelBuilder.Entity<ResourcePendingRequest>()
                .HasMany(p => p.Scopes)
                .WithOne(p => p.PendingRequest)
                .HasForeignKey(p => p.PendingRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
