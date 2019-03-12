using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF.Mappings
{
    internal static class ScopeMapping
    {
        public static void AddScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>()
                .ToTable("scopes")
                .HasKey(p => p.Name);
            modelBuilder.Entity<Scope>()
                .Property(s => s.Description)
                .HasMaxLength(255);
            modelBuilder.Entity<Scope>()
                .HasMany(s => s.ScopeClaims)
                .WithOne(c => c.Scope)
                .HasForeignKey(c => c.ScopeName)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Scope>()
                .HasMany(s => s.ClientScopes)
                .WithOne(c => c.Scope)
                .HasForeignKey(c => c.ScopeName)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Scope>()
                .HasMany(s => s.ConsentScopes)
                .WithOne(c => c.Scope)
                .HasForeignKey(c => c.ScopeName)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
