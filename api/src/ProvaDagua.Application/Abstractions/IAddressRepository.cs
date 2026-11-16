using ProvaDagua.Domain.Addresses;

namespace ProvaDagua.Application.Abstractions;

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> GetBySensorAsync(string sensorId, CancellationToken ct);
}