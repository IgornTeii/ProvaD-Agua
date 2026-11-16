using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Addresses;
using ProvaDagua.Domain.Alerts;
using ProvaDagua.Domain.Certificates;
using ProvaDagua.Domain.FloodEvents;
using ProvaDagua.Domain.Readings;
using ProvaDagua.Domain.Sensors;

namespace ProvaDagua.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<FloodEvent> FloodEvents => Set<FloodEvent>();
    public DbSet<FloodCertificate> FloodCertificates => Set<FloodCertificate>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<AlertSubscription> AlertSubscriptions => Set<AlertSubscription>();
    public DbSet<AlertNotification> AlertNotifications => Set<AlertNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
