using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.EF.Mappings
{
    internal static class AuthenticationContextclassReferenceMapping
    {
        public static ModelBuilder AddAuthenticationContextclassReferenceMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.AuthenticationContextclassReference>()
                .ToTable("acrs")
                .HasKey(c => c.Name);
            return modelBuilder;
        }
    }
}
