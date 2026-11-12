using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal static class EnterpriseMap
{
    public static void Base<T>(EntityTypeBuilder<T> b, string table) where T : TenantEntity
    {
        b.ToTable(table);
        EntityConfiguration.ConfigureTenantEntity(b);
        b.HasAlternateKey(x => new { x.TenantId, x.Id });
    }
}

internal sealed class DealProductConfiguration : IEntityTypeConfiguration<DealProduct>
{
    public void Configure(EntityTypeBuilder<DealProduct> b)
    {
        EnterpriseMap.Base(b, "DealProducts");
        b.Property(x => x.Sku).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        b.Property(x => x.TaxPercent).HasPrecision(5, 2);
        b.Property(x => x.NetAmount).HasPrecision(18, 2).IsConcurrencyToken();
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.TenantId, x.DealId, x.IsActive });
        b.HasOne<Deal>().WithMany().HasForeignKey(x => new { x.TenantId, x.DealId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Product>().WithMany().HasForeignKey(x => new { x.TenantId, x.ProductId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesQuoteConfiguration : IEntityTypeConfiguration<SalesQuote>
{
    public void Configure(EntityTypeBuilder<SalesQuote> b)
    {
        EnterpriseMap.Base(b, "SalesQuotes");
        b.Property(x => x.Number).HasMaxLength(50).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsConcurrencyToken();
        b.Property(x => x.NetAmount).HasPrecision(18, 2).IsConcurrencyToken();
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Ignore(x => x.TotalAmount);
        b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.DealId, x.Status });
        b.HasOne<Deal>().WithMany().HasForeignKey(x => new { x.TenantId, x.DealId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => new { x.TenantId, x.CustomerId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesQuoteLineConfiguration : IEntityTypeConfiguration<SalesQuoteLine>
{
    public void Configure(EntityTypeBuilder<SalesQuoteLine> b)
    {
        EnterpriseMap.Base(b, "SalesQuoteLines");
        b.Property(x => x.Sku).HasMaxLength(40).IsRequired();
        b.Property(x => x.Description).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        b.Property(x => x.TaxPercent).HasPrecision(5, 2);
        b.Property(x => x.NetAmount).HasPrecision(18, 2).IsConcurrencyToken();
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Ignore(x => x.TotalAmount);
        b.HasIndex(x => new { x.TenantId, x.QuoteId });
        b.HasOne<SalesQuote>().WithMany().HasForeignKey(x => new { x.TenantId, x.QuoteId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Product>().WithMany().HasForeignKey(x => new { x.TenantId, x.ProductId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> b)
    {
        EnterpriseMap.Base(b, "SalesOrders");
        b.Property(x => x.Number).HasMaxLength(50);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasConversion<int>().IsConcurrencyToken();
        b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.QuoteId }).IsUnique();
        b.HasOne<SalesQuote>().WithMany().HasForeignKey(x => new { x.TenantId, x.QuoteId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => new { x.TenantId, x.CustomerId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> b)
    {
        EnterpriseMap.Base(b, "SalesInvoices");
        b.Property(x => x.Number).HasMaxLength(50);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2).IsConcurrencyToken();
        b.Property(x => x.Status).HasConversion<int>().IsConcurrencyToken();
        b.Ignore(x => x.OutstandingAmount);
        b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.OrderId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Status, x.DueDate });
        b.HasOne<SalesOrder>().WithMany().HasForeignKey(x => new { x.TenantId, x.OrderId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => new { x.TenantId, x.CustomerId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesPaymentConfiguration : IEntityTypeConfiguration<SalesPayment>
{
    public void Configure(EntityTypeBuilder<SalesPayment> b)
    {
        EnterpriseMap.Base(b, "SalesPayments");
        b.Property(x => x.Reference).HasMaxLength(100);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.RecordedAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter());
        b.HasIndex(x => new { x.TenantId, x.Reference }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.InvoiceId });
        b.HasOne<SalesInvoice>().WithMany().HasForeignKey(x => new { x.TenantId, x.InvoiceId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmPipelineConfiguration : IEntityTypeConfiguration<CrmPipeline>
{
    public void Configure(EntityTypeBuilder<CrmPipeline> b)
    {
        EnterpriseMap.Base(b, "CrmPipelines");
        b.Property(x => x.Name).HasMaxLength(120);
        b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}

internal sealed class CrmPipelineStageConfiguration : IEntityTypeConfiguration<CrmPipelineStage>
{
    public void Configure(EntityTypeBuilder<CrmPipelineStage> b)
    {
        EnterpriseMap.Base(b, "CrmPipelineStages");
        b.HasAlternateKey(x => new { x.TenantId, x.PipelineId, x.Id });
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.Kind).HasConversion<int>();
        b.HasIndex(x => new { x.TenantId, x.PipelineId, x.Position }).IsUnique();
        b.HasOne<CrmPipeline>().WithMany().HasForeignKey(x => new { x.TenantId, x.PipelineId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmDealPipelinePositionConfiguration : IEntityTypeConfiguration<CrmDealPipelinePosition>
{
    public void Configure(EntityTypeBuilder<CrmDealPipelinePosition> b)
    {
        EnterpriseMap.Base(b, "CrmDealPipelinePositions");
        b.HasIndex(x => new { x.TenantId, x.DealId }).IsUnique();
        b.Property(x => x.StageId).IsConcurrencyToken();
        b.HasIndex(x => new { x.TenantId, x.PipelineId, x.StageId });
        b.HasOne<Deal>().WithMany().HasForeignKey(x => new { x.TenantId, x.DealId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<CrmPipelineStage>().WithMany().HasForeignKey(x => new { x.TenantId, x.PipelineId, x.StageId }).HasPrincipalKey(x => new { x.TenantId, x.PipelineId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmNoteConfiguration : IEntityTypeConfiguration<CrmNote>
{
    public void Configure(EntityTypeBuilder<CrmNote> b)
    {
        EnterpriseMap.Base(b, "CrmNotes");
        b.Property(x => x.RecordKind).HasConversion<int>();
        b.Property(x => x.Body).HasMaxLength(4000);
        b.HasIndex(x => new { x.TenantId, x.RecordKind, x.RecordId });
        b.HasOne<TenantMember>().WithMany().HasForeignKey(x => new { x.TenantId, x.AuthorMemberId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmTagConfiguration : IEntityTypeConfiguration<CrmTag>
{
    public void Configure(EntityTypeBuilder<CrmTag> b)
    {
        EnterpriseMap.Base(b, "CrmTags");
        b.Property(x => x.Name).HasMaxLength(80);
        b.Property(x => x.Color).HasMaxLength(7);
        b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}

internal sealed class CrmRecordTagConfiguration : IEntityTypeConfiguration<CrmRecordTag>
{
    public void Configure(EntityTypeBuilder<CrmRecordTag> b)
    {
        EnterpriseMap.Base(b, "CrmRecordTags");
        b.Property(x => x.RecordKind).HasConversion<int>();
        b.HasIndex(x => new { x.TenantId, x.TagId, x.RecordKind, x.RecordId }).IsUnique();
        b.HasOne<CrmTag>().WithMany().HasForeignKey(x => new { x.TenantId, x.TagId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmCustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CrmCustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CrmCustomFieldDefinition> b)
    {
        EnterpriseMap.Base(b, "CrmCustomFieldDefinitions");
        b.Property(x => x.RecordKind).HasConversion<int>();
        b.Property(x => x.FieldType).HasConversion<int>();
        b.Property(x => x.Key).HasMaxLength(64);
        b.Property(x => x.Label).HasMaxLength(120);
        b.HasIndex(x => new { x.TenantId, x.RecordKind, x.Key }).IsUnique();
    }
}

internal sealed class CrmCustomFieldValueConfiguration : IEntityTypeConfiguration<CrmCustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CrmCustomFieldValue> b)
    {
        EnterpriseMap.Base(b, "CrmCustomFieldValues");
        b.Property(x => x.RecordKind).HasConversion<int>();
        b.Property(x => x.Value).HasMaxLength(2000).IsConcurrencyToken();
        b.HasIndex(x => new { x.TenantId, x.FieldId, x.RecordId }).IsUnique();
        b.HasOne<CrmCustomFieldDefinition>().WithMany().HasForeignKey(x => new { x.TenantId, x.FieldId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesGoalConfiguration : IEntityTypeConfiguration<SalesGoal>
{
    public void Configure(EntityTypeBuilder<SalesGoal> b)
    {
        EnterpriseMap.Base(b, "SalesGoals");
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.TargetAmount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.TenantId, x.MemberId, x.Year, x.Month, x.Currency }).IsUnique();
        b.HasOne<TenantMember>().WithMany().HasForeignKey(x => new { x.TenantId, x.MemberId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CrmAuditEventConfiguration : IEntityTypeConfiguration<CrmAuditEvent>
{
    public void Configure(EntityTypeBuilder<CrmAuditEvent> b)
    {
        EnterpriseMap.Base(b, "CrmAuditEvents");
        b.Property(x => x.Action).HasMaxLength(80);
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.Details).HasMaxLength(2000);
        b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.CreatedAt });
        b.HasOne<TenantMember>().WithMany().HasForeignKey(x => new { x.TenantId, x.ActorMemberId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
