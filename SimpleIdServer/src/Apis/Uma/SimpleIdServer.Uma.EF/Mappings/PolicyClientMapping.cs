using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;
using System;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyClientMapping
    {
        public static ModelBuilder AddPolicyClientMapping(this ModelBuilder modelBuilder)
        {
            if(modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<PolicyClient>().ToTable("policyClients")
                .HasKey(a => new
                {
                    a.ClientId,
                    a.PolicyId
                });
            return modelBuilder;
        }
    }
}
