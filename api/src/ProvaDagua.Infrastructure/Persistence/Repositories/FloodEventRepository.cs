using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Infrastructure.Persistence.Repositories;

internal sealed class FloodEventRepository(AppDbContext db) : IFloodEventRepository
{
    public Task<FloodEvent?> GetOpenBySensorAsync(string sensorId, CancellationToken ct) =>
        db.FloodEvents.FirstOrDefaultAsync(e =>
            e.SensorId == sensorId &&
            (e.Status == FloodEventStatus.Suspected || e.Status == FloodEventStatus.Confirmed), ct);

    public async Task<IReadOnlyList<FloodEvent>> GetByStatusAsync(FloodEventStatus status, CancellationToken ct) =>
        await db.FloodEvents.Where(e => e.Status == status).ToListAsync(ct);

    public void Add(FloodEvent floodEvent) => db.FloodEvents.Add(floodEvent);
}