using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Addresses;
using ProvaDagua.Infrastructure.Persistence.Seed;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Street).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Number).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Neighborhood).HasMaxLength(100).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.SensorId).HasMaxLength(50).IsRequired();
        builder.Ignore(a => a.FullAddress);

        builder.HasIndex(a => a.SensorId);

        builder.HasData(SaoPauloSeed.Sensors.SelectMany(sensor =>
            sensor.HouseNumbers.Select((number, index) => new
            {
                Id = SaoPauloSeed.AddressId(sensor.Number, index),
                sensor.Street,
                Number = number,
                sensor.Neighborhood,
                City = "São Paulo",
                SensorId = sensor.Id
            })));
    }
}
