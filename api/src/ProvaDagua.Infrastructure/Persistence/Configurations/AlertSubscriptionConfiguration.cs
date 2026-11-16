using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Alerts;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class AlertSubscriptionConfiguration : IEntityTypeConfiguration<AlertSubscription>
{
    public void Configure(EntityTypeBuilder<AlertSubscription> builder)
    {
        builder.ToTable("alert_subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Phone).HasMaxLength(11).IsRequired();

        builder.HasIndex(s => new { s.AddressId, s.Phone }).IsUnique();
    }
}
