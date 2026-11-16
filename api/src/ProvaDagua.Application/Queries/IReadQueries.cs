using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Application.Queries;

public sealed record AddressDto(
    Guid Id, string Street, string Number, string Neighborhood, string City,
    string SensorId, string FullAddress);

public sealed record ReadingDto(
    decimal WaterLevelCm, decimal RainfallMmPerHour, DateTimeOffset MeasuredAt);

public sealed record FloodEventDto(
    Guid Id, string SensorId, string SensorName, string Neighborhood,
    double Latitude, double Longitude, string Status, decimal PeakWaterLevelCm,
    DateTimeOffset StartedAt, DateTimeOffset? ConfirmedAt,
    DateTimeOffset? ClosedAt, DateTimeOffset? DismissedAt);

public sealed record SensorStatusDto(
    string SensorId, ReadingDto? LatestReading, FloodEventDto? OpenEvent);

public sealed record SensorMapDto(
    string Id, string Name, string Neighborhood, string Zone,
    double Latitude, double Longitude, string Status, ReadingDto? LatestReading);

public sealed record CertificateDto(
    Guid Id, string VerificationCode, Guid FloodEventId, AddressDto Address,
    decimal PeakWaterLevelCm, DateTimeOffset FloodStartedAt, DateTimeOffset FloodClosedAt,
    bool IsEligibleForAid, DateTimeOffset IssuedAt);

public sealed record AlertNotificationDto(
    Guid Id, string Phone, string Message, DateTimeOffset SentAt);

public sealed record FloodEventFilter(
    string? SensorId, FloodEventStatus? Status, DateOnly? From, DateOnly? To, string? Street);

public interface IReadQueries
{
    Task<IReadOnlyList<AddressDto>> SearchAddressesAsync(string? search, CancellationToken ct);
    Task<IReadOnlyList<SensorMapDto>> GetSensorsAsync(CancellationToken ct);
    Task<SensorStatusDto> GetSensorStatusAsync(string sensorId, CancellationToken ct);
    Task<IReadOnlyList<FloodEventDto>> GetFloodEventsAsync(FloodEventFilter filter, CancellationToken ct);
    Task<FloodEventDto?> GetFloodEventAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CertificateDto>> GetCertificatesAsync(Guid? addressId, Guid? floodEventId, CancellationToken ct);
    Task<CertificateDto?> GetCertificateByCodeAsync(string verificationCode, CancellationToken ct);
    Task<IReadOnlyList<AlertNotificationDto>> GetNotificationsByAddressAsync(Guid addressId, CancellationToken ct);
}
