using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyRuleClientIdMapping
    {
        public static ModelBuilder AddPolicyRuleClientIdMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyRuleClientId>()
                .ToTable("PolicyRuleClientIds")
                .HasKey(a => new
                {
                    a.ClientId,
                    a.PolicyRuleId
                });
            return modelBuilder;
        }
    }
}
