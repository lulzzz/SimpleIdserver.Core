using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF.Mappings
{
    internal static class UserCredentialMapping
    {
        public static ModelBuilder AddUserCredentialMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.UserCredential>()
                .ToTable("userCredentials")
                .HasKey(p => new { p.UserId, p.Type });
            return modelBuilder;
        }
    }
}
