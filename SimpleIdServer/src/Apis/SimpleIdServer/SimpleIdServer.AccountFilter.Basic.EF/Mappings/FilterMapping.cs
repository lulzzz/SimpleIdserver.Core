using System;
using Microsoft.EntityFrameworkCore;
using SimpleIdServer.AccountFilter.Basic.EF.Models;

namespace SimpleIdServer.AccountFilter.Basic.EF.Mappings
{
    internal static class FilterMapping
    {
        public static ModelBuilder AddFilterMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<Filter>()
                .ToTable("filters")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Filter>()
                .HasMany(c => c.Rules)
                .WithOne(s => s.Filter)
                .HasForeignKey(s => s.FilterId)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
