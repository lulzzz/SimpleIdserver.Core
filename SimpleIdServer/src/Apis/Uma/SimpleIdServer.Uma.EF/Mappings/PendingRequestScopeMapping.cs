using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;
using System;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PendingRequestScopeMapping
    {
        public static ModelBuilder AddPendingRequestScopeMapping(this ModelBuilder modelBuilder)
        {
            if(modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<PendingRequestScope>().ToTable("pendingRequestScopes").HasKey(c => new
            {
                c.PendingRequestId,
                c.Scope
            });
            return modelBuilder;
        }
    }
}
