using Microsoft.EntityFrameworkCore;
using SimpleIdServer.IdentityStore.EF.Mappings;
using SimpleIdServer.IdentityStore.EF.Models;

namespace SimpleIdServer.IdentityStore.EF
{
    public class IdentityStoreEFContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddUserClaimMapping();
            modelBuilder.AddUserCredentialMapping();
            modelBuilder.AddUserMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}