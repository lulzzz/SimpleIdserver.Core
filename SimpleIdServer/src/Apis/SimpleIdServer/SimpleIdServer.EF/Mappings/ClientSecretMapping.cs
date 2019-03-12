using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ClientSecretMapping
    {
        public static ModelBuilder AddClientSecretMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientSecret>()
                .ToTable("ClientSecrets")
                .HasKey(s => s.Id);
            return modelBuilder;
        }
    }
}
