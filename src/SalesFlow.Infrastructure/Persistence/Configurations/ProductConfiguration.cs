using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        EntityConfiguration.ConfigureTenantEntity(builder);
        builder.Property(x => x.Sku).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasAlternateKey(x => new { x.TenantId, x.Id });
        builder.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}
