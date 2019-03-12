using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class JsonWebKeyMapping
    {
        public static void AddJsonWebKeyMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonWebKey>()
                .ToTable("jsonWebKeys")
                .HasKey(j => j.Kid);
        }
    }
}
