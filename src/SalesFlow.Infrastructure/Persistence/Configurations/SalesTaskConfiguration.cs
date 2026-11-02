using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class SalesTaskConfiguration : IEntityTypeConfiguration<SalesTask>
{
    public void Configure(EntityTypeBuilder<SalesTask> builder)
    {
        builder.ToTable("SalesTasks");
        EntityConfiguration.ConfigureTenantEntity(builder);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.DueAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter());
        builder.Property(x => x.CompletedAt).HasColumnType("timestamp without time zone").HasConversion(new LocalDateTimeConverter());
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.AssignedMemberId });
        builder.HasOne<TenantMember>().WithMany().HasForeignKey(x => new { x.TenantId, x.AssignedMemberId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
