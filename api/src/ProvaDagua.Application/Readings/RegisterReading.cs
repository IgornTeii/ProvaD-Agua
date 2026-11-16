using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Common;
using ProvaDagua.Domain.FloodEvents;
using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Application.Readings;

public sealed record RegisterReadingCommand(string SensorId, decimal WaterLevelCm, decimal RainfallMmPerHour, DateTimeOffset MeasuredAt);

public sealed class RegisterReadingHandler(
    IReadingRepository readings,
    IFloodEventRepository floodEvents,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Result> HandleAsync(RegisterReadingCommand command, CancellationToken ct)
    {
        var measuredAtUtc = command.MeasuredAt.ToUniversalTime();

        if (await readings.ExistsAsync(command.SensorId, measuredAtUtc, ct))
            return Result.Success();

        var readingResult = Reading.Create(command.SensorId, command.WaterLevelCm, command.RainfallMmPerHour,
            measuredAtUtc, clock.GetUtcNow());

        if (!readingResult.IsSuccess)
            return Result.Failure(readingResult.Error!);

        var reading = readingResult.Value!;
        readings.Add(reading);

        var openEvent = await floodEvents.GetOpenBySensorAsync(reading.SensorId, ct);

        if (openEvent is not null)
        {
            var recordResult = openEvent.RecordWaterLevel(reading.WaterLevelCm);
            
            if (!recordResult.IsSuccess)
                return recordResult;
        }
        else if (reading.WaterLevelCm >= FloodEvent.SuspicionThresholdCm)
        {
            var startResult = FloodEvent.Start(reading.SensorId, reading.WaterLevelCm, reading.MeasuredAt);
            if (!startResult.IsSuccess)
                return Result.Failure(startResult.Error!);

            floodEvents.Add(startResult.Value!);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}