using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deals");
        EntityConfiguration.ConfigureTenantEntity(builder);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Stage).HasConversion<int>().IsRequired();
        builder.Property(x => x.ClosedAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter());
        builder.HasIndex(x => new { x.TenantId, x.Stage });
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => new { x.TenantId, x.CustomerId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
