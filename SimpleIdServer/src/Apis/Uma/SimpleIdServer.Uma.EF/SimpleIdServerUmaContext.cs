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
        public virtual DbSet<PolicyRule> PolicyRules { get; set; }
        public virtual DbSet<ShareResourceLink> ShareResourceLinks { get; set; }

        #endregion

        #region Protected methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddResourceSetMappings();
            modelBuilder.AddPolicyMappings();
            modelBuilder.AddPolicyRuleMappings();
            modelBuilder.AddPolicyResourceMappings();
            modelBuilder.AddShareResourceLinkMapping();
            modelBuilder.AddPolicyRuleClaimMapping();
            modelBuilder.AddPolicyRuleClientIdMapping();
            modelBuilder.AddPolicyRuleScopeMapping();
            modelBuilder.AddResourceScopeMapping();
            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}
