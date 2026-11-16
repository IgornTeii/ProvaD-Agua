using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class ReadingConfiguration : IEntityTypeConfiguration<Reading>
{
    public void Configure(EntityTypeBuilder<Reading> builder)
    {
        builder.ToTable("readings");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SensorId).HasMaxLength(50).IsRequired();
        builder.Property(r => r.WaterLevelCm).HasPrecision(6, 2);
        builder.Property(r => r.RainfallMmPerHour).HasPrecision(6, 2);
        builder.HasIndex(r => new { r.SensorId, r.MeasuredAt }).IsUnique();
    }
}