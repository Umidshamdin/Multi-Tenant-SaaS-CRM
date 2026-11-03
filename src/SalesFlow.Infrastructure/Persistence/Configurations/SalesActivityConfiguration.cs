using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class SalesActivityConfiguration
    : IEntityTypeConfiguration<SalesActivity>
{
    public void Configure(EntityTypeBuilder<SalesActivity> builder)
    {
        builder.ToTable("SalesActivities");

        EntityConfiguration.ConfigureTenantEntity(builder);

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.CreatedByMemberId)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.OccurredAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CustomerId,
            x.OccurredAtUtc
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.DealId
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CreatedByMemberId
        });

        // Activity -> Customer
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.TenantId,
                x.CustomerId
            })
            .HasPrincipalKey(x => new
            {
                x.TenantId,
                x.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        // Activity -> Deal (optional)
        builder.HasOne<Deal>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.TenantId,
                x.DealId
            })
            .HasPrincipalKey(x => new
            {
                x.TenantId,
                x.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        // Activity -> Employee
        builder.HasOne<TenantMember>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.TenantId,
                x.CreatedByMemberId
            })
            .HasPrincipalKey(x => new
            {
                x.TenantId,
                x.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
