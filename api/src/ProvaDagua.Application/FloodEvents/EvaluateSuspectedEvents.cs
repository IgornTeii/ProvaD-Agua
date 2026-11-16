using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Alerts;
using ProvaDagua.Domain.Common;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Application.FloodEvents;

public sealed class EvaluateSuspectedEventsHandler(
    IFloodEventRepository floodEvents,
    IReadingRepository readings,
    IFloodClassifier classifier,
    IAlertRepository alerts,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public const int WindowSize = 30;

    public async Task<int> HandleAsync(CancellationToken ct)
    {
        var suspectedEvents = await floodEvents.GetByStatusAsync(FloodEventStatus.Suspected, ct);
        var decided = 0;

        foreach (var floodEvent in suspectedEvents)
        {
            var window = await readings.GetLatestAsync(floodEvent.SensorId, WindowSize, ct);
            var classification = await classifier.ClassifyAsync(window, ct);
            var now = clock.GetUtcNow();

            Result? result = classification switch
            {
                FloodClassification.Flood => floodEvent.Confirm(now),
                FloodClassification.Noise => floodEvent.Dismiss(now),
                _ => null
            };

            if (result is not { IsSuccess: true })
                continue;

            if (floodEvent.Status == FloodEventStatus.Confirmed)
                await NotifySubscribersAsync(floodEvent, now, ct);

            decided++;
        }

        if (decided > 0)
            await unitOfWork.SaveChangesAsync(ct);

        return decided;
    }

    private async Task NotifySubscribersAsync(FloodEvent floodEvent, DateTimeOffset now, CancellationToken ct)
    {
        var subscribers = await alerts.GetSubscribersBySensorAsync(floodEvent.SensorId, ct);

        foreach (var subscriber in subscribers)
            alerts.Add(AlertNotification.FloodConfirmed(subscriber.Subscription, subscriber.Street, floodEvent.Id, now));
    }
}
