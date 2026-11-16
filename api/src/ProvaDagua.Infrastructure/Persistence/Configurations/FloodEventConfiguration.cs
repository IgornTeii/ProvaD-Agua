using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class FloodEventConfiguration : IEntityTypeConfiguration<FloodEvent>
{
    public void Configure(EntityTypeBuilder<FloodEvent> builder)
    {
        builder.ToTable("flood_events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SensorId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PeakWaterLevelCm).HasPrecision(6, 2);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.SensorId)
            .IsUnique()
            .HasFilter("\"Status\" IN ('Suspected', 'Confirmed')");
    }
}