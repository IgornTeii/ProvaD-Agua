using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.FloodEvents;

public sealed class FloodEvent
{
    public const decimal SuspicionThresholdCm = 15m;
    public Guid Id { get; private set; }
    public string SensorId { get; private set; } = default!;
    public FloodEventStatus Status { get; private set; }
    public decimal PeakWaterLevelCm { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }

    private FloodEvent() { } // EF Core

    public static Result<FloodEvent> Start(string sensorId, decimal initialWaterLevelCm, DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
            return Result<FloodEvent>.Failure(FloodEventErrors.SensorRequired);
        if (initialWaterLevelCm < 0)
            return Result<FloodEvent>.Failure(FloodEventErrors.InvalidWaterLevel);

        return Result<FloodEvent>.Success(new FloodEvent
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Status = FloodEventStatus.Suspected,
            PeakWaterLevelCm = initialWaterLevelCm,
            StartedAt = startedAt
        });
    }

    public Result Confirm(DateTimeOffset confirmedAt)
    {
        if (Status != FloodEventStatus.Suspected)
            return Result.Failure(FloodEventErrors.OnlySuspectedCanBeConfirmed);

        Status = FloodEventStatus.Confirmed;
        ConfirmedAt = confirmedAt;
        return Result.Success();
    }

    public Result Close(DateTimeOffset closedAt)
    {
        if (Status != FloodEventStatus.Confirmed)
            return Result.Failure(FloodEventErrors.OnlyConfirmedCanBeClosed);
        if (closedAt < StartedAt)
            return Result.Failure(FloodEventErrors.ClosedBeforeStart);

        Status = FloodEventStatus.Closed;
        ClosedAt = closedAt;
        return Result.Success();
    }

    public Result Dismiss(DateTimeOffset dismissedAt)
    {
        if (Status != FloodEventStatus.Suspected)
            return Result.Failure(FloodEventErrors.OnlySuspectedCanBeDismissed);

        Status = FloodEventStatus.Dismissed;
        DismissedAt = dismissedAt;
        return Result.Success();
    }

    public Result RecordWaterLevel(decimal waterLevelCm)
    {
        if (Status == FloodEventStatus.Closed)
            return Result.Failure(FloodEventErrors.EventAlreadyClosed);
        if (waterLevelCm < 0)
            return Result.Failure(FloodEventErrors.InvalidWaterLevel);

        if (waterLevelCm > PeakWaterLevelCm)
            PeakWaterLevelCm = waterLevelCm;

        return Result.Success();
    }
}