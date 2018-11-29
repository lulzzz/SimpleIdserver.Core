using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class ShareResourceLinkMappings
    {
        public static ModelBuilder AddShareResourceLinkMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShareResourceLink>()
                .ToTable("ShareResourceLinks")
                .HasKey(a => a.ConfirmationCode);
            return modelBuilder;
        }
    }
}
