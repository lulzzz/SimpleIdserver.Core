﻿using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class ResourceSetMappings
    {
        #region Public static methods

        public static void AddResourceSetMappings(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceSet>()
                .ToTable("ResourceSets")
                .HasKey(r => r.Id);
            modelBuilder.Entity<ResourceSet>()
                .HasMany(p => p.ResourceSetPolicies)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
