using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal static class EntityConfiguration
{
    public static void ConfigureEntity<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : Entity
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter()).IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter());
    }

    public static void ConfigureTenantEntity<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : TenantEntity
    {
        ConfigureEntity(builder);
        builder.Property(x => x.TenantId).IsRequired();
        builder.HasIndex(x => x.TenantId);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}
