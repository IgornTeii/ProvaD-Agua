using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Alerts;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class AlertNotificationConfiguration : IEntityTypeConfiguration<AlertNotification>
{
    public void Configure(EntityTypeBuilder<AlertNotification> builder)
    {
        builder.ToTable("alert_notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Phone).HasMaxLength(11).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(400).IsRequired();

        builder.HasIndex(n => n.AddressId);
        builder.HasIndex(n => new { n.SubscriptionId, n.FloodEventId }).IsUnique();
    }
}
