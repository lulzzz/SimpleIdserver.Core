using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Mappings;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF
{
    public class SimpleIdServerUmaContext : DbContext
    {
        #region Constructor

        public SimpleIdServerUmaContext(DbContextOptions<SimpleIdServerUmaContext> dbContextOptions):base(dbContextOptions)
        {
        }

        #endregion

        #region Properties

        public virtual DbSet<ResourceSet> ResourceSets { get; set; }
        public virtual DbSet<Policy> Policies { get; set; }
        public virtual DbSet<ShareResourceLink> ShareResourceLinks { get; set; }
        public virtual DbSet<ResourceSetPolicy> ResourceSetPolicies { get; set; }

        #endregion

        #region Protected methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddResourceSetMappings();
            modelBuilder.AddPolicyMappings();
            modelBuilder.AddResourceSetPolicyMapping();
            modelBuilder.AddShareResourceLinkMapping();
            modelBuilder.AddPolicyClaimMapping();
            modelBuilder.AddPolicyScopeMapping();
            modelBuilder.AddResourceScopeMapping();
            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}
