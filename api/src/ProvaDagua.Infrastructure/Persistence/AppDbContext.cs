using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Certificates;
using ProvaDagua.Domain.FloodEvents;
using ProvaDagua.Domain.Readings;
using ProvaDagua.Domain.Addresses;

namespace ProvaDagua.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<FloodEvent> FloodEvents => Set<FloodEvent>();
    public DbSet<FloodCertificate> FloodCertificates => Set<FloodCertificate>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}