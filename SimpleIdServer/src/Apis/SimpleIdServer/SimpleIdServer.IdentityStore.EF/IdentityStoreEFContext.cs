using Microsoft.EntityFrameworkCore;

namespace SimpleIdServer.IdentityStore.EF
{
    public class IdentityStoreEFContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddClaimMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}