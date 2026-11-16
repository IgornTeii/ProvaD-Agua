using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Queries;
using ProvaDagua.Domain.Addresses;
using ProvaDagua.Domain.Certificates;
using ProvaDagua.Domain.FloodEvents;
using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Infrastructure.Persistence;

internal sealed class ReadQueries(AppDbContext db) : IReadQueries
{
    public async Task<IReadOnlyList<AddressDto>> SearchAddressesAsync(string? search, CancellationToken ct)
    {
        var query = db.Addresses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Street, term) || EF.Functions.ILike(a.Neighborhood, term));
        }

        var addresses = await query.OrderBy(a => a.Street).ThenBy(a => a.Number).ToListAsync(ct);
        return addresses.Select(ToDto).ToList();
    }

    public async Task<SensorStatusDto> GetSensorStatusAsync(string sensorId, CancellationToken ct)
    {
        var latestReading = await db.Readings.AsNoTracking()
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.MeasuredAt)
            .FirstOrDefaultAsync(ct);

        var openEvent = await db.FloodEvents.AsNoTracking()
            .Where(e => e.SensorId == sensorId &&
                        (e.Status == FloodEventStatus.Suspected || e.Status == FloodEventStatus.Confirmed))
            .FirstOrDefaultAsync(ct);

        return new SensorStatusDto(
            sensorId,
            latestReading is null ? null : ToDto(latestReading),
            openEvent is null ? null : ToDto(openEvent));
    }

    public async Task<IReadOnlyList<FloodEventDto>> GetFloodEventsAsync(
        string? sensorId, FloodEventStatus? status, CancellationToken ct)
    {
        var query = db.FloodEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(sensorId))
            query = query.Where(e => e.SensorId == sensorId);
        if (status is not null)
            query = query.Where(e => e.Status == status);

        var events = await query.OrderByDescending(e => e.StartedAt).ToListAsync(ct);
        return events.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<CertificateDto>> GetCertificatesByAddressAsync(Guid addressId, CancellationToken ct)
    {
        var rows = await (
            from certificate in db.FloodCertificates.AsNoTracking()
            join address in db.Addresses.AsNoTracking() on certificate.AddressId equals address.Id
            where certificate.AddressId == addressId
            orderby certificate.IssuedAt descending
            select new { certificate, address }).ToListAsync(ct);

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

    private static AddressDto ToDto(Address a) =>
        new(a.Id, a.Street, a.Number, a.Neighborhood, a.City, a.SensorId, a.FullAddress);

    private static ReadingDto ToDto(Reading r) =>
        new(r.WaterLevelCm, r.RainfallMmPerHour, r.MeasuredAt);

    private static FloodEventDto ToDto(FloodEvent e) =>
        new(e.Id, e.SensorId, e.Status.ToString(), e.PeakWaterLevelCm,
            e.StartedAt, e.ConfirmedAt, e.ClosedAt, e.DismissedAt);

    private static CertificateDto ToDto(FloodCertificate c, Address a) =>
        new(c.Id, c.VerificationCode, c.FloodEventId, ToDto(a),
            c.PeakWaterLevelCm, c.FloodStartedAt, c.FloodClosedAt,
            c.IsEligibleForAid, c.IssuedAt);
}