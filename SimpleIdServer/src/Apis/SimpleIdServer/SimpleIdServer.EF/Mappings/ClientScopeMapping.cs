using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ClientScopeMapping
    {
        public static void AddClientScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientScope>()
                .ToTable("clientScopes")
                .HasKey(s => new { s.ClientId, s.ScopeName });
        }
    }
}