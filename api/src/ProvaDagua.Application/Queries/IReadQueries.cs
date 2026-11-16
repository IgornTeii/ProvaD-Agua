using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Application.Queries;

public sealed record AddressDto(
    Guid Id, string Street, string Number, string Neighborhood, string City,
    string SensorId, string FullAddress);

public sealed record ReadingDto(
    decimal WaterLevelCm, decimal RainfallMmPerHour, DateTimeOffset MeasuredAt);

public sealed record FloodEventDto(
    Guid Id, string SensorId, string Status, decimal PeakWaterLevelCm,
    DateTimeOffset StartedAt, DateTimeOffset? ConfirmedAt,
    DateTimeOffset? ClosedAt, DateTimeOffset? DismissedAt);

public sealed record SensorStatusDto(
    string SensorId, ReadingDto? LatestReading, FloodEventDto? OpenEvent);

public sealed record CertificateDto(
    Guid Id, string VerificationCode, Guid FloodEventId, AddressDto Address,
    decimal PeakWaterLevelCm, DateTimeOffset FloodStartedAt, DateTimeOffset FloodClosedAt,
    bool IsEligibleForAid, DateTimeOffset IssuedAt);

public interface IReadQueries
{
    Task<IReadOnlyList<AddressDto>> SearchAddressesAsync(string? search, CancellationToken ct);
    Task<SensorStatusDto> GetSensorStatusAsync(string sensorId, CancellationToken ct);
    Task<IReadOnlyList<FloodEventDto>> GetFloodEventsAsync(string? sensorId, FloodEventStatus? status, CancellationToken ct);
    Task<IReadOnlyList<CertificateDto>> GetCertificatesByAddressAsync(Guid addressId, CancellationToken ct);
    Task<CertificateDto?> GetCertificateByCodeAsync(string verificationCode, CancellationToken ct);
}