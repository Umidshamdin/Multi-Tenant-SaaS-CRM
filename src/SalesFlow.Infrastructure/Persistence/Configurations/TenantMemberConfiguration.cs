using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesFlow.Domain.Entities;
using SalesFlow.Infrastructure.Identity;

namespace SalesFlow.Infrastructure.Persistence.Configurations;

internal sealed class TenantMemberConfiguration : IEntityTypeConfiguration<TenantMember>
{
    public void Configure(EntityTypeBuilder<TenantMember> builder)
    {
        builder.ToTable("TenantMembers");
        EntityConfiguration.ConfigureTenantEntity(builder);
        builder.Property(x => x.IdentityUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Role).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.IdentityUserId }).IsUnique();
        builder.HasAlternateKey(x => new { x.TenantId, x.Id });
        builder.HasOne<SalesFlowUser>().WithMany().HasForeignKey(x => x.IdentityUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
