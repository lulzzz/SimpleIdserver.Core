using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyRuleScopeMapping
    {
        public static ModelBuilder AddPolicyRuleScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyRuleScope>()
                .ToTable("PolicyRuleScopes")
                .HasKey(a => new
                {
                    a.Scope,
                    a.PolicyRuleId
                });
            return modelBuilder;
        }
    }
}
