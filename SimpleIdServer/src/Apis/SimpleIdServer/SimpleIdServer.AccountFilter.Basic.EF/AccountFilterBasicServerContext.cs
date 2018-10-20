using Microsoft.EntityFrameworkCore;
using SimpleIdServer.AccountFilter.Basic.EF.Mappings;
using SimpleIdServer.AccountFilter.Basic.EF.Models;

namespace SimpleIdServer.AccountFilter.Basic.EF
{
    public class AccountFilterBasicServerContext : DbContext
    {
        #region Constructor

        public AccountFilterBasicServerContext(DbContextOptions<AccountFilterBasicServerContext> dbContextOptions):base(dbContextOptions)
        {
        }

        #endregion

        public DbSet<Filter> Filters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFilterMapping()
                .AddFilterRuleMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}