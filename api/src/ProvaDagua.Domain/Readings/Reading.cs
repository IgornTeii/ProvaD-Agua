using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.Readings;

public sealed class Reading
{
    public const decimal MaxWaterLevelCm = 500m;

    public Guid Id { get; private set; }
    public string SensorId { get; private set; } = default!;
    public decimal WaterLevelCm { get; private set; }
    public decimal RainfallMmPerHour { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }

    private Reading() { }

    public static Result<Reading> Create(string sensorId, decimal waterLevelCm, decimal rainfallMmPerHour,
        DateTimeOffset measuredAt, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
            return Result<Reading>.Failure(ReadingErrors.SensorRequired);
        if (waterLevelCm is < 0 or > MaxWaterLevelCm)
            return Result<Reading>.Failure(ReadingErrors.InvalidWaterLevel);
        if (rainfallMmPerHour < 0)
            return Result<Reading>.Failure(ReadingErrors.InvalidRainfall);
        if (measuredAt > now.AddMinutes(5))
            return Result<Reading>.Failure(ReadingErrors.MeasuredInFuture);

        return Result<Reading>.Success(new Reading
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            WaterLevelCm = waterLevelCm,
            RainfallMmPerHour = rainfallMmPerHour,
            MeasuredAt = measuredAt,
            ReceivedAt = now
        });
    }
}