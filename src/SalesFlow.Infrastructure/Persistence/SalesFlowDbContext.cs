
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

    // Core
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<TenantMember> TenantMembers => Set<TenantMember>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();

    // Leads & Deals
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealProduct> DealProducts => Set<DealProduct>();

    // Tasks & Activities
    public DbSet<SalesTask> SalesTasks => Set<SalesTask>();
    public DbSet<SalesActivity> SalesActivities => Set<SalesActivity>();

    // Products
    public DbSet<Product> Products => Set<Product>();

    // Quotes
    public DbSet<SalesQuote> SalesQuotes => Set<SalesQuote>();
    public DbSet<SalesQuoteLine> SalesQuoteLines => Set<SalesQuoteLine>();

    // Orders & Billing
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesPayment> SalesPayments => Set<SalesPayment>();

    // Pipelines
    public DbSet<CrmPipeline> CrmPipelines => Set<CrmPipeline>();
    public DbSet<CrmPipelineStage> CrmPipelineStages => Set<CrmPipelineStage>();
    public DbSet<CrmDealPipelinePosition> CrmDealPipelinePositions => Set<CrmDealPipelinePosition>();

    // Notes & Tags
    public DbSet<CrmNote> CrmNotes => Set<CrmNote>();
    public DbSet<CrmTag> CrmTags => Set<CrmTag>();
    public DbSet<CrmRecordTag> CrmRecordTags => Set<CrmRecordTag>();

    // Custom Fields
    public DbSet<CrmCustomFieldDefinition> CrmCustomFieldDefinitions => Set<CrmCustomFieldDefinition>();
    public DbSet<CrmCustomFieldValue> CrmCustomFieldValues => Set<CrmCustomFieldValue>();

    // Sales Goals
    public DbSet<SalesGoal> SalesGoals => Set<SalesGoal>();

    // Audit
    public DbSet<CrmAuditEvent> CrmAuditEvents => Set<CrmAuditEvent>();

    public SalesFlowDbContext(DbContextOptions<SalesFlowDbContext> options, ICurrentUser currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesFlowDbContext).Assembly);

        // Core
        ConfigureFilter<Organization>(modelBuilder);
        ConfigureFilter<TenantMember>(modelBuilder);

        // Customers
        ConfigureFilter<Customer>(modelBuilder);
        ConfigureFilter<CustomerContact>(modelBuilder);

        // Leads & Deals
        ConfigureFilter<Lead>(modelBuilder);
        ConfigureFilter<Deal>(modelBuilder);
        ConfigureFilter<DealProduct>(modelBuilder);

        // Tasks & Activities
        ConfigureFilter<SalesTask>(modelBuilder);
        ConfigureFilter<SalesActivity>(modelBuilder);

        // Products
        ConfigureFilter<Product>(modelBuilder);

        // Quotes
        ConfigureFilter<SalesQuote>(modelBuilder);
        ConfigureFilter<SalesQuoteLine>(modelBuilder);

        // Orders & Billing
        ConfigureFilter<SalesOrder>(modelBuilder);
        ConfigureFilter<SalesInvoice>(modelBuilder);
        ConfigureFilter<SalesPayment>(modelBuilder);

        // Pipelines
        ConfigureFilter<CrmPipeline>(modelBuilder);
        ConfigureFilter<CrmPipelineStage>(modelBuilder);
        ConfigureFilter<CrmDealPipelinePosition>(modelBuilder);

        // Notes & Tags
        ConfigureFilter<CrmNote>(modelBuilder);
        ConfigureFilter<CrmTag>(modelBuilder);
        ConfigureFilter<CrmRecordTag>(modelBuilder);

        // Custom Fields
        ConfigureFilter<CrmCustomFieldDefinition>(modelBuilder);
        ConfigureFilter<CrmCustomFieldValue>(modelBuilder);

        // Sales Goals
        ConfigureFilter<SalesGoal>(modelBuilder);

        // Audit
        ConfigureFilter<CrmAuditEvent>(modelBuilder);

        // Identity
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

        var tenantEntries = ChangeTracker.Entries<Tenant>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        var dataEntries = ChangeTracker.Entries<TenantEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        if (tenantEntries.Length == 0 && dataEntries.Length == 0) return;

        var selectedTenantId = _currentUser.TenantId;

        if (selectedTenantId is null || selectedTenantId == Guid.Empty)
        {
            if (!IsValidOnboarding(tenantEntries, dataEntries)) throw new ForbiddenException("A selected tenant is required for CRM writes.");
            return;
        }

        if (tenantEntries.Any(x => x.State == EntityState.Added || x.Entity.Id != selectedTenantId.Value))
            throw new ForbiddenException("Cannot modify another tenant.");

        foreach (var entry in dataEntries)
        {
            if (entry.Entity.TenantId != selectedTenantId.Value)
                throw new ForbiddenException("Cross-tenant write denied.");

            if (entry.State is EntityState.Modified or EntityState.Deleted &&
                entry.Property(nameof(TenantEntity.TenantId)).OriginalValue is Guid originalTenantId &&
                originalTenantId != selectedTenantId.Value)
                throw new ForbiddenException("Tenant changes are not allowed.");
        }
    }

    private bool IsValidOnboarding(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Tenant>[] tenants,
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TenantEntity>[] entries)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId) || tenants.Length != 1 || tenants[0].State != EntityState.Added || entries.Length != 2) return false;

        var tenant = tenants[0].Entity;

        var organization = entries.SingleOrDefault(x => x.Entity is Organization && x.State == EntityState.Added)?.Entity as Organization;
        var owner = entries.SingleOrDefault(x => x.Entity is TenantMember && x.State == EntityState.Added)?.Entity as TenantMember;

        return organization?.TenantId == tenant.Id &&
               owner?.TenantId == tenant.Id &&
               owner.Role == MemberRole.Owner &&
               owner.IdentityUserId == _currentUser.UserId;
    }
}
