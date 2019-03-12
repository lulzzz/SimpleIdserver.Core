using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF.Mappings
{
    public static class CredentialSettingMapping
    {
        public static ModelBuilder AddCredentialSettingMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.CredentialSetting>()
                .ToTable("credentialSettings")
                .HasKey(c => c.CredentialType);
            return modelBuilder;
        }
    }
}
