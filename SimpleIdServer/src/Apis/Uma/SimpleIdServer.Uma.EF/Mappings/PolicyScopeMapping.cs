using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyScopeMapping
    {
        public static ModelBuilder AddPolicyScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyScope>()
                .ToTable("PolicyScopes")
                .HasKey(a => new
                {
                    a.Scope,
                    a.PolicyId
                });
            return modelBuilder;
        }
    }
}
