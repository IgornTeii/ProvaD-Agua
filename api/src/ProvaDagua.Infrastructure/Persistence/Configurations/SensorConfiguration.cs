using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Sensors;
using ProvaDagua.Infrastructure.Persistence.Seed;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasMaxLength(50);
        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Neighborhood).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Zone).HasMaxLength(20).IsRequired();

        builder.HasData(SaoPauloSeed.Sensors.Select(s => new
        {
            s.Id,
            s.Name,
            s.Neighborhood,
            s.Zone,
            s.Latitude,
            s.Longitude
        }));
    }
}
