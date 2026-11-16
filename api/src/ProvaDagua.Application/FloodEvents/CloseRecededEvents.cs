using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Certificates;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Application.FloodEvents;

public sealed class CloseRecededEventsHandler(
    IFloodEventRepository floodEvents,
    IReadingRepository readings,
    IAddressRepository addresses,
    IFloodCertificateRepository certificates,
    IVerificationCodeGenerator verificationCodes,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<int> HandleAsync(CancellationToken ct)
    {
        var confirmedEvents = await floodEvents.GetByStatusAsync(FloodEventStatus.Confirmed, ct);
        var closed = 0;

        foreach (var floodEvent in confirmedEvents)
        {
            var latest = await readings.GetLatestAsync(
                floodEvent.SensorId, FloodRecessionPolicy.ReadingsRequired, ct);

            if (!FloodRecessionPolicy.HasReceded(latest))
                continue;

            var closeResult = floodEvent.Close(latest[0].MeasuredAt);
            if (!closeResult.IsSuccess)
                continue;

            var issuedAt = clock.GetUtcNow();
            var coveredAddresses = await addresses.GetBySensorAsync(floodEvent.SensorId, ct);

            foreach (var address in coveredAddresses)
            {
                var certificate = FloodCertificate.Issue(
                    floodEvent, address.Id, verificationCodes.Generate(), issuedAt);

                if (certificate.IsSuccess)
                    certificates.Add(certificate.Value!);
            }

            closed++;
        }

        if (closed > 0)
            await unitOfWork.SaveChangesAsync(ct);

        return closed;
    }
}