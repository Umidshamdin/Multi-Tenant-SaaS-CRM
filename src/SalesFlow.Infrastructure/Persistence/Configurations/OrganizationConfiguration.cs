using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        EntityConfiguration.ConfigureTenantEntity(builder);
        builder.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(500);
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}
