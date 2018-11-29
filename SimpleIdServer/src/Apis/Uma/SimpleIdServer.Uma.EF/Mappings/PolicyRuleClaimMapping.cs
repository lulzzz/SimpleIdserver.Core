using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyRuleClaimMapping
    {
        public static ModelBuilder AddPolicyRuleClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyRuleClaim>()
                .ToTable("PolicyRuleClaims")
                .HasKey(a => new
                {
                    a.Key,
                    a.PolicyRuleId
                });
            return modelBuilder;
        }
    }
}
