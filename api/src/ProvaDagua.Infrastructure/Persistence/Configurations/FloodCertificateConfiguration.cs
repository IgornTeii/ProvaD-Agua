
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaDagua.Domain.Certificates;

namespace ProvaDagua.Infrastructure.Persistence.Configurations;

internal sealed class FloodCertificateConfiguration : IEntityTypeConfiguration<FloodCertificate>
{
    public void Configure(EntityTypeBuilder<FloodCertificate> builder)
    {
        builder.ToTable("flood_certificates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.VerificationCode).HasMaxLength(20).IsRequired();
        builder.Property(c => c.PeakWaterLevelCm).HasPrecision(6, 2);
        builder.HasIndex(c => c.VerificationCode).IsUnique();
        builder.HasIndex(c => new { c.FloodEventId, c.AddressId }).IsUnique();
    }
}