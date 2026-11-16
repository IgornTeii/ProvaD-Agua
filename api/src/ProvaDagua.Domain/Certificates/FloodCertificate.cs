using ProvaDagua.Domain.Common;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Domain.Certificates;

public sealed class FloodCertificate
{
    public const decimal AidEligibilityThresholdCm = 30m;

    public Guid Id { get; private set; }
    public Guid FloodEventId { get; private set; }
    public Guid AddressId { get; private set; }
    public string VerificationCode { get; private set; } = default!;
    public decimal PeakWaterLevelCm { get; private set; }
    public DateTimeOffset FloodStartedAt { get; private set; }
    public DateTimeOffset FloodClosedAt { get; private set; }

    public bool IsEligibleForAid { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }

    private FloodCertificate() { }

    public static Result<FloodCertificate> Issue(
        FloodEvent floodEvent, Guid addressId, string verificationCode, DateTimeOffset issuedAt)
    {
        if (addressId == Guid.Empty)
            return Result<FloodCertificate>.Failure(FloodCertificateErrors.AddressRequired);
        if (string.IsNullOrWhiteSpace(verificationCode))
            return Result<FloodCertificate>.Failure(FloodCertificateErrors.VerificationCodeRequired);
        if (floodEvent.Status != FloodEventStatus.Closed || floodEvent.ClosedAt is null)
            return Result<FloodCertificate>.Failure(FloodCertificateErrors.EventNotClosed);

        return Result<FloodCertificate>.Success(new FloodCertificate
        {
            Id = Guid.NewGuid(),
            FloodEventId = floodEvent.Id,
            AddressId = addressId,
            VerificationCode = verificationCode,
            PeakWaterLevelCm = floodEvent.PeakWaterLevelCm,
            FloodStartedAt = floodEvent.StartedAt,
            FloodClosedAt = floodEvent.ClosedAt.Value,
            IsEligibleForAid = floodEvent.PeakWaterLevelCm >= AidEligibilityThresholdCm,
            IssuedAt = issuedAt
        });
    }
}