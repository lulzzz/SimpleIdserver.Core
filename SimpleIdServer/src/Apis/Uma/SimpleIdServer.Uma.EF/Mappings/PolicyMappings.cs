using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyMappings
    {
        #region Public static methods

        public static void AddPolicyMappings(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Policy>()
                .ToTable("Policies")
                .HasKey(a => a.Id);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.ResourceSetPolicies)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Claims)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Scopes)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Clients)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
