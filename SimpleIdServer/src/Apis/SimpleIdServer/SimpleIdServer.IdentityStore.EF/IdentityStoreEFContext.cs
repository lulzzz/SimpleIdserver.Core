using Microsoft.EntityFrameworkCore;
using SimpleIdServer.IdentityStore.EF.Mappings;
using SimpleIdServer.IdentityStore.EF.Models;

namespace SimpleIdServer.IdentityStore.EF
{
    public class IdentityStoreEFContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserCredential> UserCredentials { get; set; }
        public virtual DbSet<CredentialSetting> CredentialSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddUserClaimMapping();
            modelBuilder.AddUserCredentialMapping();
            modelBuilder.AddUserMapping();
            modelBuilder.AddCredentialSettingMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}