using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;
using SalesFlow.Infrastructure.Identity;

namespace SalesFlow.Infrastructure.Persistence;

public sealed class SalesFlowDbContext : IdentityDbContext<SalesFlowUser>, IUnitOfWork
{
    private readonly ICurrentUser _currentUser;
    public Guid? CurrentTenantId => _currentUser.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<TenantMember> TenantMembers => Set<TenantMember>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<SalesTask> SalesTasks => Set<SalesTask>();

    public DbSet<SalesActivity> SalesActivities => Set<SalesActivity>();

    public SalesFlowDbContext(DbContextOptions<SalesFlowDbContext> options, ICurrentUser currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesFlowDbContext).Assembly);
        ConfigureFilter<Organization>(modelBuilder);
        ConfigureFilter<TenantMember>(modelBuilder);
        ConfigureFilter<Customer>(modelBuilder);
        ConfigureFilter<CustomerContact>(modelBuilder);
        ConfigureFilter<Lead>(modelBuilder);
        ConfigureFilter<Deal>(modelBuilder);
        ConfigureFilter<SalesTask>(modelBuilder);
        ConfigureFilter<SalesActivity>(modelBuilder);
        modelBuilder.Entity<SalesFlowUser>().Property(x => x.DisplayName).HasMaxLength(160);
    }

    private void ConfigureFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
    }

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        VerifyTenantWrites();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        VerifyTenantWrites();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void VerifyTenantWrites()
    {
        ChangeTracker.DetectChanges();
        var tenantEntries = ChangeTracker.Entries<Tenant>().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        var dataEntries = ChangeTracker.Entries<TenantEntity>().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        if (tenantEntries.Length == 0 && dataEntries.Length == 0) return; // Identity-only operations.

        var selectedTenantId = _currentUser.TenantId;
        if (selectedTenantId is null || selectedTenantId == Guid.Empty)
        {
            if (!IsValidOnboarding(tenantEntries, dataEntries)) throw new ForbiddenException("A selected tenant is required for CRM writes.");
            return;
        }

        if (tenantEntries.Any(x => x.State == EntityState.Added || x.Entity.Id != selectedTenantId.Value)) throw new ForbiddenException("Cannot modify another tenant.");
        foreach (var entry in dataEntries)
        {
            if (entry.Entity.TenantId != selectedTenantId.Value) throw new ForbiddenException("Cross-tenant write denied.");
            if (entry.State == EntityState.Modified && (Guid)entry.Property(nameof(TenantEntity.TenantId)).OriginalValue! != selectedTenantId.Value) throw new ForbiddenException("Tenant changes are not allowed.");
        }
    }

    private bool IsValidOnboarding(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Tenant>[] tenants, Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TenantEntity>[] entries)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId) || tenants.Length != 1 || tenants[0].State != EntityState.Added || entries.Length != 2) return false;
        var tenant = tenants[0].Entity;
        var organization = entries.SingleOrDefault(x => x.Entity is Organization && x.State == EntityState.Added)?.Entity as Organization;
        var owner = entries.SingleOrDefault(x => x.Entity is TenantMember && x.State == EntityState.Added)?.Entity as TenantMember;
        return organization?.TenantId == tenant.Id && owner?.TenantId == tenant.Id && owner.Role == MemberRole.Owner && owner.IdentityUserId == _currentUser.UserId;
    }
}
