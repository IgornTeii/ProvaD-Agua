using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Queries;
using ProvaDagua.Domain.Addresses;
using ProvaDagua.Domain.Certificates;
using ProvaDagua.Domain.FloodEvents;
using ProvaDagua.Domain.Readings;
using ProvaDagua.Domain.Sensors;

namespace ProvaDagua.Infrastructure.Persistence;

internal sealed class ReadQueries(AppDbContext db) : IReadQueries
{
    private static readonly TimeSpan SaoPauloOffset = TimeSpan.FromHours(-3);

    public async Task<IReadOnlyList<AddressDto>> SearchAddressesAsync(string? search, CancellationToken ct)
    {
        var query = db.Addresses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Street, term) || EF.Functions.ILike(a.Neighborhood, term));
        }

        var addresses = await query.OrderBy(a => a.Street).ThenBy(a => a.Number).ToListAsync(ct);
        return addresses.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<SensorMapDto>> GetSensorsAsync(CancellationToken ct)
    {
        var sensors = await db.Sensors.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);
        var openEvents = await OpenEventsQuery().ToListAsync(ct);
        var result = new List<SensorMapDto>();

        foreach (var sensor in sensors)
        {
            var latest = await LatestReadingAsync(sensor.Id, ct);
            var status = openEvents.FirstOrDefault(e => e.SensorId == sensor.Id)?.Status.ToString() ?? "Normal";

            result.Add(new SensorMapDto(
                sensor.Id, sensor.Name, sensor.Neighborhood, sensor.Zone,
                sensor.Latitude, sensor.Longitude, status,
                latest is null ? null : ToDto(latest)));
        }

        return result;
    }

    public async Task<SensorStatusDto> GetSensorStatusAsync(string sensorId, CancellationToken ct)
    {
        var sensor = await db.Sensors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sensorId, ct);
        var latest = await LatestReadingAsync(sensorId, ct);
        var openEvent = await OpenEventsQuery().FirstOrDefaultAsync(e => e.SensorId == sensorId, ct);

        return new SensorStatusDto(
            sensorId,
            latest is null ? null : ToDto(latest),
            openEvent is null || sensor is null ? null : ToDto(openEvent, sensor));
    }

    public async Task<IReadOnlyList<FloodEventDto>> GetFloodEventsAsync(FloodEventFilter filter, CancellationToken ct)
    {
        var query = db.FloodEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SensorId))
            query = query.Where(e => e.SensorId == filter.SensorId);

        if (filter.Status is not null)
            query = query.Where(e => e.Status == filter.Status);

        if (filter.From is { } from)
        {
            var start = StartOfDay(from);
            query = query.Where(e => e.StartedAt >= start);
        }

        if (filter.To is { } to)
        {
            var end = StartOfDay(to.AddDays(1));
            query = query.Where(e => e.StartedAt < end);
        }

        if (!string.IsNullOrWhiteSpace(filter.Street))
        {
            var term = $"%{filter.Street.Trim()}%";
            var sensorIds = db.Addresses
                .Where(a => EF.Functions.ILike(a.Street, term) || EF.Functions.ILike(a.Neighborhood, term))
                .Select(a => a.SensorId);
            query = query.Where(e => sensorIds.Contains(e.SensorId));
        }

        var events = await query.OrderByDescending(e => e.StartedAt).Take(500).ToListAsync(ct);
        var sensors = await db.Sensors.AsNoTracking().ToDictionaryAsync(s => s.Id, ct);

        return events
            .Where(e => sensors.ContainsKey(e.SensorId))
            .Select(e => ToDto(e, sensors[e.SensorId]))
            .ToList();
    }

    public async Task<FloodEventDto?> GetFloodEventAsync(Guid id, CancellationToken ct)
    {
        var floodEvent = await db.FloodEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (floodEvent is null)
            return null;

        var sensor = await db.Sensors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == floodEvent.SensorId, ct);
        return sensor is null ? null : ToDto(floodEvent, sensor);
    }

    public async Task<IReadOnlyList<CertificateDto>> GetCertificatesAsync(
        Guid? addressId, Guid? floodEventId, CancellationToken ct)
    {
        var query =
            from certificate in db.FloodCertificates.AsNoTracking()
            join address in db.Addresses.AsNoTracking() on certificate.AddressId equals address.Id
            select new { certificate, address };

        if (addressId is not null)
            query = query.Where(r => r.certificate.AddressId == addressId);

        if (floodEventId is not null)
            query = query.Where(r => r.certificate.FloodEventId == floodEventId);

        var rows = await query
            .OrderByDescending(r => r.certificate.IssuedAt)
            .ThenBy(r => r.address.Street)
            .ThenBy(r => r.address.Number)
            .ToListAsync(ct);

        return rows.Select(r => ToDto(r.certificate, r.address)).ToList();
    }

    public async Task<CertificateDto?> GetCertificateByCodeAsync(string verificationCode, CancellationToken ct)
    {
        var code = verificationCode.Trim().ToUpperInvariant();

        var row = await (
            from certificate in db.FloodCertificates.AsNoTracking()
            join address in db.Addresses.AsNoTracking() on certificate.AddressId equals address.Id
            where certificate.VerificationCode == code
            select new { certificate, address }).FirstOrDefaultAsync(ct);

        return row is null ? null : ToDto(row.certificate, row.address);
    }

    public async Task<IReadOnlyList<AlertNotificationDto>> GetNotificationsByAddressAsync(Guid addressId, CancellationToken ct)
    {
        var notifications = await db.AlertNotifications.AsNoTracking()
            .Where(n => n.AddressId == addressId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync(ct);

        return notifications
            .Select(n => new AlertNotificationDto(n.Id, MaskPhone(n.Phone), n.Message, n.SentAt))
            .ToList();
    }

    private IQueryable<FloodEvent> OpenEventsQuery() =>
        db.FloodEvents.AsNoTracking().Where(e =>
            e.Status == FloodEventStatus.Suspected || e.Status == FloodEventStatus.Confirmed);

    private Task<Reading?> LatestReadingAsync(string sensorId, CancellationToken ct) =>
        db.Readings.AsNoTracking()
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.MeasuredAt)
            .FirstOrDefaultAsync(ct);

    private static DateTimeOffset StartOfDay(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), SaoPauloOffset).ToUniversalTime();

    private static string MaskPhone(string digits) =>
        digits.Length >= 4 ? $"({digits[..2]}) •••••-{digits[^4..]}" : "•••";

    private static AddressDto ToDto(Address a) =>
        new(a.Id, a.Street, a.Number, a.Neighborhood, a.City, a.SensorId, a.FullAddress);

    private static ReadingDto ToDto(Reading r) =>
        new(r.WaterLevelCm, r.RainfallMmPerHour, r.MeasuredAt);

    private static FloodEventDto ToDto(FloodEvent e, Sensor s) =>
        new(e.Id, e.SensorId, s.Name, s.Neighborhood, s.Latitude, s.Longitude,
            e.Status.ToString(), e.PeakWaterLevelCm,
            e.StartedAt, e.ConfirmedAt, e.ClosedAt, e.DismissedAt);

    private static CertificateDto ToDto(FloodCertificate c, Address a) =>
        new(c.Id, c.VerificationCode, c.FloodEventId, ToDto(a),
            c.PeakWaterLevelCm, c.FloodStartedAt, c.FloodClosedAt,
            c.IsEligibleForAid, c.IssuedAt);
}
