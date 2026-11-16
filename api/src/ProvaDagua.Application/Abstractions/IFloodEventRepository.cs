// IFloodEventRepository.cs
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Application.Abstractions;

public interface IFloodEventRepository
{
    Task<FloodEvent?> GetOpenBySensorAsync(string sensorId, CancellationToken ct);
    Task<IReadOnlyList<FloodEvent>> GetByStatusAsync(FloodEventStatus status, CancellationToken ct);
    void Add(FloodEvent floodEvent);
}