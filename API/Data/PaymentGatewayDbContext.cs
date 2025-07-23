using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PaymentGatewayDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TenantSettings> TenantSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tenant configuration
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Subdomain).IsUnique();
                entity.HasIndex(e => e.ContactEmail).IsUnique();

                entity.HasMany(e => e.Users)
                    .WithOne(e => e.Tenant)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.PaymentAccounts)
                    .WithOne(e => e.Tenant)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.Tenant)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Settings)
                    .WithOne(e => e.Tenant)
                    .HasForeignKey<TenantSettings>(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

                entity.Property(e => e.Role)
                    .HasConversion<int>();
            });

            // PaymentAccount configuration
            modelBuilder.Entity<PaymentAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.AccountName });

                entity.Property(e => e.Provider)
                    .HasConversion<int>();

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.PaymentAccount)
                    .HasForeignKey(e => e.PaymentAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.ExternalTransactionId);
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Status)
                    .HasConversion<int>();

                entity.Property(e => e.Type)
                    .HasConversion<int>();
            });

            // TenantSettings configuration
            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId).IsUnique();
            });

            // Global query filters for multi-tenancy
            ApplyGlobalFilters(modelBuilder);
        }

        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            // Apply tenant isolation for all tenant-specific entities
            modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
            modelBuilder.Entity<PaymentAccount>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
            modelBuilder.Entity<Transaction>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
            modelBuilder.Entity<TenantSettings>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
        }

        public override int SaveChanges()
        {
            SetTenantId();
            SetAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantId();
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantId()
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            if (currentTenantId == 0) return;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added && entry.Entity is not Tenant)
                {
                    var tenantIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
                    if (tenantIdProperty != null && (int)tenantIdProperty.CurrentValue == 0)
                    {
                        tenantIdProperty.CurrentValue = currentTenantId;
                    }
                }
            }
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProperty != null)
                    {
                        createdAtProperty.CurrentValue = DateTime.UtcNow;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProperty != null)
                    {
                        updatedAtProperty.CurrentValue = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
