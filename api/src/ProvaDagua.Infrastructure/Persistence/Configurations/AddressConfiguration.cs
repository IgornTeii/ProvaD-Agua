using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Addresses;

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
        builder.HasData(
            Seed("11111111-1111-1111-1111-000000000001", "Rua das Flores", "10", "sensor-01"),
            Seed("11111111-1111-1111-1111-000000000002", "Rua das Flores", "25", "sensor-01"),
            Seed("11111111-1111-1111-1111-000000000003", "Rua das Flores", "40", "sensor-01"),
            Seed("11111111-1111-1111-1111-000000000004", "Rua das Flores", "57", "sensor-01"),
            Seed("22222222-2222-2222-2222-000000000001", "Rua dos Ipês", "5", "sensor-02"),
            Seed("22222222-2222-2222-2222-000000000002", "Rua dos Ipês", "18", "sensor-02"),
            Seed("22222222-2222-2222-2222-000000000003", "Rua dos Ipês", "33", "sensor-02"));
    }

    private static object Seed(string id, string street, string number, string sensorId) => new
    {
        Id = Guid.Parse(id),
        Street = street,
        Number = number,
        Neighborhood = "Jardim das Águas",
        City = "São Paulo",
        SensorId = sensorId
    };
}