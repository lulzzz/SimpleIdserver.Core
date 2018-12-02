using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyClaimMapping
    {
        public static ModelBuilder AddPolicyClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyClaim>()
                .ToTable("PolicyClaims")
                .HasKey(a => new
                {
                    a.Key,
                    a.PolicyId
                });
            return modelBuilder;
        }
    }
}
