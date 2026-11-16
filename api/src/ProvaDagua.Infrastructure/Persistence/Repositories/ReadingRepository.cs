using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Infrastructure.Persistence.Repositories;

internal sealed class ReadingRepository(AppDbContext db) : IReadingRepository
{
    public Task<bool> ExistsAsync(string sensorId, DateTimeOffset measuredAt, CancellationToken ct) =>
        db.Readings.AnyAsync(r => r.SensorId == sensorId && r.MeasuredAt == measuredAt, ct);

    public async Task<IReadOnlyList<Reading>> GetLatestAsync(string sensorId, int count, CancellationToken ct)
    {
        var latest = await db.Readings
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.MeasuredAt)
            .Take(count)
            .ToListAsync(ct);

        latest.Reverse();
        
        return latest;
    }

    public void Add(Reading reading) => db.Readings.Add(reading);
}