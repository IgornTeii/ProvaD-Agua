using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Application.Abstractions;

public interface IReadingRepository
{
    Task<bool> ExistsAsync(string sensorId, DateTimeOffset measuredAt, CancellationToken ct);
    Task<IReadOnlyList<Reading>> GetLatestAsync(string sensorId, int count, CancellationToken ct);
    void Add(Reading reading);
}