using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Addresses;

namespace ProvaDagua.Infrastructure.Persistence.Repositories;

internal sealed class AddressRepository(AppDbContext db) : IAddressRepository
{
    public async Task<IReadOnlyList<Address>> GetBySensorAsync(string sensorId, CancellationToken ct) =>
        await db.Addresses.Where(a => a.SensorId == sensorId).ToListAsync(ct);
}