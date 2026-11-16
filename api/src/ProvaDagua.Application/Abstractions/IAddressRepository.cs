using ProvaDagua.Domain.Addresses;

namespace ProvaDagua.Application.Abstractions;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Address>> GetBySensorAsync(string sensorId, CancellationToken ct);
}
