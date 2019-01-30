using Microsoft.EntityFrameworkCore;
using SimpleIdServer.EF.Mappings;
using SimpleIdServer.EF.Models;

namespace SimpleIdServer.EF
{
    public class SimpleIdentityServerContext : DbContext
    {
        #region Constructor

        public SimpleIdentityServerContext(DbContextOptions<SimpleIdentityServerContext> dbContextOptions):base(dbContextOptions)
        {
        }

        #endregion

        public virtual DbSet<Translation> Translations { get; set; }
        public virtual DbSet<Scope> Scopes { get; set; }
        public virtual DbSet<Claim> Claims { get; set; }
        public virtual DbSet<ResourceOwner> ResourceOwners { get; set; }
        public virtual DbSet<JsonWebKey> JsonWebKeys { get; set; } 
        public virtual DbSet<Models.Client> Clients { get; set; } 
        public virtual DbSet<Consent> Consents { get; set; }
        public virtual DbSet<ClientScope> ClientScopes { get; set; }        
        public virtual DbSet<ConsentClaim> ConsentClaims { get; set; }
        public virtual DbSet<ConsentScope> ConsentScopes { get; set; }
        public virtual DbSet<ScopeClaim> ScopeClaims { get; set; }
        public virtual DbSet<ResourceOwnerClaim> ResourceOwnerClaims { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<CredentialSetting> CredentialSettings { get; set; }
        public virtual DbSet<DefaultSettings> DefaultSettings { get; set; }
        public virtual DbSet<AuthenticationContextclassReference> AuthenticationContextclassReferences { get; set; }
        public virtual DbSet<ResourceOwnerCredential> ResourceOwnerCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddClaimMapping();
            modelBuilder.AddClientMapping();
            modelBuilder.AddConsentClaimMapping();
            modelBuilder.AddConsentMapping();
            modelBuilder.AddConsentScopeMapping();
            modelBuilder.AddJsonWebKeyMapping();
            modelBuilder.AddResourceOwnerMapping();
            modelBuilder.AddScopeClaimMapping();
            modelBuilder.AddScopeMapping();
            modelBuilder.AddTranslationMapping();
            modelBuilder.AddClientScopeMapping();
            modelBuilder.AddResourceOwnerClaimMapping();
            modelBuilder.AddClientSecretMapping();
            modelBuilder.AddProfileMapping();
            modelBuilder.AddCredentialsSettingsMapping();
            modelBuilder.AddResourceCredentialMapping();
            modelBuilder.AddDefaultSettingsMapping();
            modelBuilder.AddAuthenticationContextclassReferenceMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}