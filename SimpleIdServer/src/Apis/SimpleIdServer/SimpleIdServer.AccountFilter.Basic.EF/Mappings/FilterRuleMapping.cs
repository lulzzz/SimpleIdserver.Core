using System;
using Microsoft.EntityFrameworkCore;
using SimpleIdServer.AccountFilter.Basic.EF.Models;

namespace SimpleIdServer.AccountFilter.Basic.EF.Mappings
{
    internal static class FilterRuleMapping
    {
        public static ModelBuilder AddFilterRuleMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<FilterRule>()
                .ToTable("filterRules")
                .HasKey(c => c.Id);
            return modelBuilder;
        }
    }
}
